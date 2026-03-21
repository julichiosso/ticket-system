import { defineStore } from 'pinia';
import axios from 'axios';
import { API_URL } from './auth';

const statusMap = {
  0: 'Pending',
  1: 'InProgress',
  2: 'Resolved',
  3: 'Closed',
  4: 'WaitingForUser'
};

function safeArray(value) {
  return Array.isArray(value) ? value : [];
}

function normalizeComment(comment) {
  if (!comment) return null;
  return {
    id: comment.id ?? comment.Id,
    ticketId: comment.ticketId ?? comment.TicketId,
    authorId: comment.authorId ?? comment.autorId ?? comment.AutorId,
    author: comment.author ?? comment.autor ?? comment.Autor ?? 'System',
    authorRole: comment.authorRole ?? comment.autorRol ?? comment.rol ?? comment.Rol ?? null,
    message: comment.message ?? comment.mensaje ?? comment.Mensaje ?? '',
    isInternal: comment.isInternal ?? comment.interno ?? comment.Interno ?? false,
    createdAt: comment.createdAt ?? comment.fecha ?? comment.Fecha ?? new Date().toISOString(),
    attachments: safeArray(comment.attachments ?? comment.adjuntos ?? comment.Adjuntos ?? []).map(a => ({
      id: a.id ?? a.Id,
      url: a.url ?? a.Url ?? a.ruta ?? a.Ruta ?? '',
      originalName: a.originalName ?? a.nombreOriginal ?? a.NombreOriginal ?? a.nombre ?? a.Nombre ?? '',
    }))
  };
}

export const useTicketsStore = defineStore('tickets', {
  state: () => ({
    tickets: [],
    totalTickets: 0,
    currentPage: 1,
    pageSize: 25,
    totalPages: 1,
    filters: {
      title: '',
      status: null,
      priority: null,
      operatorId: null,
      userId: null,
      dateFrom: null,
      dateTo: null,
      sortBy: 'createdAt',
      descending: true,
    },
    selectedTicket: null,
    comments: [],
    loading: false,
    loadingComments: false,
  }),
  actions: {
    async fetchAll(extraFilters = {}) {
      this.loading = true;
      try {
        const params = {
          Page: this.currentPage,
          PageSize: this.pageSize,
          SortBy: this.filters.sortBy,
          Descending: this.filters.descending,
          ...this._activeFilters(),
          ...extraFilters,
        };
        Object.keys(params).forEach(k => {
          if (params[k] === null || params[k] === '' || params[k] === undefined)
            delete params[k];
        });
        const response = await axios.get(`${API_URL}/tickets`, { params });
        const result = response.data?.data;
        this.tickets = safeArray(result?.data ?? result);
        this.totalTickets = result?.totalRecords ?? this.tickets.length;
        this.totalPages = Math.ceil(this.totalTickets / this.pageSize) || 1;
        return this.tickets;
      } finally {
        this.loading = false;
      }
    },
    async fetchMine() {
      this.loading = true;
      try {
        const response = await axios.get(`${API_URL}/tickets/my-tickets`, {
          params: { page: this.currentPage, pageSize: this.pageSize }
        });
        const result = response.data?.data;
        this.tickets = safeArray(result?.data ?? result);
        this.totalTickets = result?.totalRecords ?? this.tickets.length;
        this.totalPages = Math.ceil(this.totalTickets / this.pageSize) || 1;
        return this.tickets;
      } finally {
        this.loading = false;
      }
    },
    async fetchMineOperator() {
      this.loading = true;
      try {
        const response = await axios.get(`${API_URL}/tickets/operator/my-tickets`, {
          params: { page: this.currentPage, pageSize: this.pageSize }
        });
        const result = response.data?.data;
        this.tickets = safeArray(result?.data ?? result);
        this.totalTickets = result?.totalRecords ?? this.tickets.length;
        this.totalPages = Math.ceil(this.totalTickets / this.pageSize) || 1;
        return this.tickets;
      } finally {
        this.loading = false;
      }
    },
    async goToPage(page) {
      if (page < 1 || page > this.totalPages) return;
      this.currentPage = page;
      await this.fetchAll();
    },
    async changePageSize(size) {
      this.pageSize = size;
      this.currentPage = 1;
      await this.fetchAll();
    },
    async applyFilters(newFilters = {}) {
      this.filters = { ...this.filters, ...newFilters };
      this.currentPage = 1;
      await this.fetchAll();
    },
    async clearFilters() {
      this.filters = {
        title: '',
        status: null,
        priority: null,
        operatorId: null,
        userId: null,
        dateFrom: null,
        dateTo: null,
        sortBy: 'createdAt',
        descending: true,
      };
      this.currentPage = 1;
      await this.fetchAll();
    },
    async sort(field) {
      if (this.filters.sortBy === field) {
        this.filters.descending = !this.filters.descending;
      } else {
        this.filters.sortBy = field;
        this.filters.descending = true;
      }
      this.currentPage = 1;
      await this.fetchAll();
    },
    _activeFilters() {
      return {
        Title: this.filters.title || null,
        Status: this.filters.status,
        Priority: this.filters.priority,
        OperatorId: this.filters.operatorId,
        UserId: this.filters.userId,
        DateFrom: this.filters.dateFrom,
        DateTo: this.filters.dateTo,
      };
    },
    async updateStatus(ticketId, newStatus) {
      try {
        const res = await axios.patch(
          `${API_URL}/tickets/${ticketId}/status`,
          { status: newStatus },
          { headers: { 'Content-Type': 'application/json' } }
        );
        const newState = statusMap[newStatus] ?? newStatus;
        const index = this.tickets.findIndex(t => t.id === ticketId);
        if (index !== -1) {
          this.tickets[index] = { ...this.tickets[index], status: newState };
        }
        if (this.selectedTicket?.id === ticketId)
          this.selectedTicket = { ...this.selectedTicket, status: newState };
      } catch (e) {
        console.error('[updateStatus] ERROR:', e.response?.status, e.response?.data);
        throw e;
      }
    },
    async deleteTicket(id) {
      await axios.delete(`${API_URL}/tickets/${id}`);
      this.tickets = this.tickets.filter(t => t.id !== id);
      this.totalTickets = Math.max(0, this.totalTickets - 1);
      this.totalPages = Math.ceil(this.totalTickets / this.pageSize) || 1;
    },
    async createTicket(data) {
      const response = await axios.post(`${API_URL}/tickets`, data);
      return response.data;
    },
    async assignOperator(ticketId, operatorId) {
      await axios.put(`${API_URL}/tickets/${ticketId}/assign`, {
        ticketGuid: ticketId,
        operatorId: operatorId === '' ? null : operatorId
      });
      const ticket = this.tickets.find(t => t.id === ticketId);
      if (ticket) ticket.assignedOperatorId = operatorId || null;
      if (this.selectedTicket?.id === ticketId)
        this.selectedTicket = { ...this.selectedTicket, assignedOperatorId: operatorId || null };
    },
    async fetchComments(ticketId) {
      this.loadingComments = true;
      try {
        const response = await axios.get(`${API_URL}/tickets/${ticketId}/comments`);
        const raw = response.data?.data ?? response.data ?? [];
        this.comments = safeArray(raw).map(normalizeComment).filter(Boolean);
        if (this.selectedTicket?.id === ticketId)
          this.selectedTicket = { ...this.selectedTicket, _comments: this.comments };
        return this.comments;
      } finally {
        this.loadingComments = false;
      }
    },
    async addComment(ticketId, payload) {
      const response = await axios.post(`${API_URL}/tickets/${ticketId}/comments`, {
        message: payload.message,
        isInternal: !!payload.isInternal
      });
      const saved = normalizeComment(response.data?.data ?? response.data);
      this.comments = [...this.comments, saved];
      const ticket = this.tickets.find(t => t.id === ticketId);
      if (ticket) ticket._comments = [...safeArray(ticket._comments), saved];
      if (this.selectedTicket?.id === ticketId)
        this.selectedTicket = { ...this.selectedTicket, _comments: [...safeArray(this.selectedTicket._comments), saved] };
      return saved;
    },
    selectTicket(ticket) {
      this.selectedTicket = ticket;
      this.comments = safeArray(ticket?._comments);
    },
    clearSelection() {
      this.selectedTicket = null;
      this.comments = [];
    },
    async fetchMetrics() {
      const response = await axios.get(`${API_URL}/metrics`);
      return response.data?.data;
    },
    async fetchAuditLogs() {
      const response = await axios.get(`${API_URL}/audit`);
      return response.data?.data;
    }
  },
  getters: {
    byStatus: (state) => (status) =>
      state.tickets.filter(t => t.status === status),
    hasActiveFilters: (state) =>
      !!(state.filters.title || state.filters.status !== null ||
        state.filters.priority !== null || state.filters.operatorId ||
        state.filters.userId || state.filters.dateFrom || state.filters.dateTo),
    stats: (state) => ({
      total: state.tickets.length,
      pending: state.tickets.filter(t => t.status === 'Pending' || t.status === 0).length,
      inProgress: state.tickets.filter(t => t.status === 'InProgress' || t.status === 1).length,
      resolved: state.tickets.filter(t => t.status === 'Resolved' || t.status === 2).length,
      closed: state.tickets.filter(t => t.status === 'Closed' || t.status === 3).length,
      waiting: state.tickets.filter(t => t.status === 'WaitingForUser' || t.status === 4).length,
      urgent: state.tickets.filter(t => t.priority === 'High' || t.priority === 2).length,
    })
  }
});
