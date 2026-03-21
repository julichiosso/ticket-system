<template>
  <div class="japanese-box space-y-6">
    <h3 class="text-lg font-black text-slate-900 dark:text-white">Export Data</h3>
    
    <div class="grid md:grid-cols-2 gap-4">
      <div class="space-y-2">
        <label class="text-xs font-bold text-slate-500 uppercase">Data Type</label>
        <select v-model="dataType" class="w-full bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-700 rounded-lg px-3 py-2 text-sm">
          <option value="tickets">Tickets</option>
          <option value="users">Users</option>
          <option value="audit">Audit</option>
          <option value="all">All Data</option>
        </select>
      </div>
      <div class="space-y-2">
        <label class="text-xs font-bold text-slate-500 uppercase">Date Range</label>
        <div class="flex gap-2">
          <input type="date" v-model="dateFrom" class="flex-1 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-700 rounded-lg px-3 py-2 text-sm" />
          <input type="date" v-model="dateTo" class="flex-1 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-700 rounded-lg px-3 py-2 text-sm" />
        </div>
      </div>
    </div>
    
    <div class="space-y-2">
      <label class="text-xs font-bold text-slate-500 uppercase">Fields to Include</label>
      <div class="grid grid-cols-2 md:grid-cols-3 gap-2">
        <label v-for="field in availableFields" :key="field" class="flex items-center gap-2 cursor-pointer hover:bg-white dark:bg-slate-900 p-2 rounded">
          <input type="checkbox" v-model="selectedFields" :value="field" class="rounded" />
          <span class="text-sm text-slate-700">{{ field }}</span>
        </label>
      </div>
    </div>
    
    <div class="grid grid-cols-2 md:grid-cols-4 gap-3">
      <button @click="exportExcel" class="px-3 py-2 bg-emerald-600 hover:bg-emerald-500 text-slate-900 dark:text-white rounded-lg text-sm font-bold transition flex items-center justify-center gap-2">
        <FileXlsxIcon class="w-4 h-4" />
        Excel
      </button>
      <button @click="exportPdf" class="px-3 py-2 bg-red-600 hover:bg-red-500 text-slate-900 dark:text-white rounded-lg text-sm font-bold transition flex items-center justify-center gap-2">
        <FileIcon class="w-4 h-4" />
        PDF
      </button>
      <button @click="printData" class="px-3 py-2 bg-blue-600 hover:bg-blue-500 text-white rounded-lg text-sm font-bold transition flex items-center justify-center gap-2">
        <PrinterIcon class="w-4 h-4" />
        Print
      </button>
      <button @click="exportJson" class="px-3 py-2 bg-purple-600 hover:bg-purple-500 text-slate-900 dark:text-white rounded-lg text-sm font-bold transition flex items-center justify-center gap-2">
        <CodeIcon class="w-4 h-4" />
        JSON
      </button>
    </div>
    
    <div v-if="previewData.length > 0" class="space-y-2">
      <label class="text-xs font-bold text-slate-500 uppercase">Preview</label>
      <div class="bg-slate-50 dark:bg-slate-950 rounded-lg p-3 max-h-48 overflow-auto border border-slate-200 dark:border-slate-700">
        <div class="text-xs text-slate-700 space-y-1">
          <div v-for="(item, idx) in previewData.slice(0, 5)" :key="idx" class="flex gap-2 pb-2 border-b border-slate-200 dark:border-slate-700 last:border-0">
            <span class="text-slate-500">{{ idx + 1 }}.</span>
            <span>{{ JSON.stringify(item).substring(0, 80) }}...</span>
          </div>
        </div>
        <div v-if="previewData.length > 5" class="text-xs text-slate-500 mt-2">+ {{ previewData.length - 5 }} more</div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { FileIcon, PrinterIcon, CodeIcon } from 'lucide-vue-next';

const props = defineProps({
  tickets: Array,
  users: Array,
  auditLog: Array
});

const dataType = ref('tickets');
const dateFrom = ref('');
const dateTo = ref('');
const selectedFields = ref(['ID', 'Title', 'User', 'Status', 'Date']);
const availableFields = ref(['ID', 'Title', 'Description', 'User', 'Status', 'Priority', 'Date', 'Comments']);

const previewData = computed(() => {
  let data = [];
  if (dataType.value === 'tickets' && props.tickets) {
    data = props.tickets.map(t => ({
      ID: t.id?.substring(0, 8),
      Title: t.title,
      User: t.userName,
      Operator: t.assignedOperatorName || 'Unassigned',
      Status: t.status,
      Priority: t.priority,
      Date: new Date(t.createdAt).toLocaleString('en-US')
    }));
  } else if (dataType.value === 'users' && props.users) {
    data = props.users.map(u => ({
      ID: u.id?.substring(0, 8),
      Name: u.name,
      Email: u.email,
      Role: u.role
    }));
  } else if (dataType.value === 'audit' && props.auditLog) {
    data = props.auditLog.map(l => ({
      Date: new Date(l.timestamp).toLocaleString('en-US'),
      Message: l.message,
      Action: l.type
    }));
  }
  return data;
});

const exportExcel = () => {
  const csv = convertToCSV(previewData.value);
  downloadFile(csv, `${dataType.value}_${new Date().toISOString().split('T')[0]}.csv`, 'text/csv');
};

const exportJson = () => {
  const json = JSON.stringify(previewData.value, null, 2);
  downloadFile(json, `${dataType.value}_${new Date().toISOString().split('T')[0]}.json`, 'application/json');
};

const exportPdf = () => {
  const content = previewData.value.map(item => JSON.stringify(item)).join('\n');
  downloadFile(content, `${dataType.value}_${new Date().toISOString().split('T')[0]}.txt`, 'text/plain');
};

const printData = () => {
  const html = `
    <table border="1" cellpadding="5" cellspacing="0" style="width:100%; border-collapse: collapse; font-family: sans-serif;">
      <thead>
        <tr style="background: #f8fafc;">${Object.keys(previewData.value[0] || {}).map(f => '<th>' + f + '</th>').join('')}</tr>
      </thead>
      <tbody>
        ${previewData.value.map(item =>
          '<tr>' + Object.values(item).map(v => '<td>' + v + '</td>').join('') + '</tr>'
        ).join('')}
      </tbody>
    </table>
  `;
  const win = window.open('', '', 'width=800,height=600');
  win.document.write('<html><head><title>Data Export</title></head><body><h2 style="font-family: sans-serif;">Report of ' + dataType.value + '</h2>' + html + '</body></html>');
  win.document.close();
  setTimeout(() => {
    win.print();
    win.close();
  }, 500);
};

const convertToCSV = (data) => {
  if (!data || data.length === 0) return '';
  const headers = Object.keys(data[0]);
  const rows = data.map(item => headers.map(h => item[h]).join(','));
  return [headers.join(','), ...rows].join('\n');
};

const downloadFile = (content, filename, type) => {
  const blob = new Blob([content], { type });
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  window.URL.revokeObjectURL(url);
  document.body.removeChild(a);
};

const FileXlsxIcon = FileIcon;
</script>
