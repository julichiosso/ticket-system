<template>
  <div class="rounded-2xl border transition-colors"
    :class="'bg-white dark:bg-slate-900 border-slate-200 dark:bg-slate-900 dark:border-slate-800'">
    
    <div class="flex items-center justify-between px-5 py-3 border-b cursor-pointer"
      :class="'border-slate-100 dark:border-slate-800'"
      @click="expanded = !expanded">
      <div class="flex items-center gap-3">
        <SlidersHorizontalIcon class="w-4 h-4 text-blue-500" />
        <span class="text-xs font-black uppercase tracking-widest">Filters</span>
        <span v-if="ticketsStore.hasActiveFilters"
          class="w-2 h-2 rounded-full bg-blue-500"></span>
      </div>
      <div class="flex items-center gap-3">
        <button v-if="ticketsStore.hasActiveFilters"
          @click.stop="ticketsStore.clearFilters()"
          class="text-[10px] font-black uppercase tracking-widest text-rose-400 hover:text-rose-300 transition-colors">
          Clear
        </button>
        <ChevronDownIcon class="w-4 h-4 transition-transform"
          :class="[expanded ? 'rotate-180' : '', settingsStore.isDark ? 'text-slate-500' : 'text-slate-400']" />
      </div>
    </div>
    
    <div v-if="expanded" class="p-5 grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">
  
      <div class="col-span-2 relative">
        <label class="block text-[9px] font-black uppercase tracking-widest mb-1.5 invisible">
          Search
        </label>
        <div class="relative">
          <SearchIcon class="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2"
            :class="'text-slate-400 dark:text-slate-600'" />
          <input v-model="localFilters.title" @input="debouncedApply"
            placeholder="Search by title..."
            class="w-full border pl-9 pr-4 py-2.5 rounded-xl text-sm outline-none transition"
            :class="'bg-slate-50 dark:bg-slate-950 border-slate-200 dark:border-slate-700 text-slate-900 dark:text-white placeholder-slate-400 focus:border-blue-500 dark:bg-slate-800 dark:border-slate-700 dark:text-white dark:placeholder-slate-600 dark:focus:border-blue-500'" />
        </div>
      </div>
   
      <div>
        <label class="block text-[9px] font-black uppercase tracking-widest mb-1.5"
          :class="'text-slate-400 dark:text-slate-600'">Status</label>
        <select v-model="localFilters.status" @change="applyFilters"
          class="w-full border px-3 py-2.5 rounded-xl text-sm outline-none transition"
          :class="'bg-slate-50 dark:bg-slate-950 border-slate-200 dark:border-slate-700 text-slate-900 dark:text-white focus:border-blue-500 dark:bg-slate-800 dark:border-slate-700 dark:text-white dark:focus:border-blue-500'">
          <option :value="null">All</option>
          <option value="0">Pending</option>
          <option value="1">In Progress</option>
          <option value="2">Resolved</option>
          <option value="3">Closed</option>
          <option value="4">Waiting for User</option>
        </select>
      </div>
      
      <div>
        <label class="block text-[9px] font-black uppercase tracking-widest mb-1.5"
          :class="'text-slate-400 dark:text-slate-600'">Priority</label>
        <select v-model="localFilters.priority" @change="applyFilters"
          class="w-full border px-3 py-2.5 rounded-xl text-sm outline-none transition"
          :class="'bg-slate-50 dark:bg-slate-950 border-slate-200 dark:border-slate-700 text-slate-900 dark:text-white focus:border-blue-500 dark:bg-slate-800 dark:border-slate-700 dark:text-white dark:focus:border-blue-500'">
          <option :value="null">All</option>
          <option value="0">Low</option>
          <option value="1">Medium</option>
          <option value="2">High</option>
        </select>
      </div>
      
      <div>
        <label class="block text-[9px] font-black uppercase tracking-widest mb-1.5"
          :class="'text-slate-400 dark:text-slate-600'">From</label>
        <input v-model="localFilters.dateFrom" @change="applyFilters" type="date"
          class="w-full border px-3 py-2.5 rounded-xl text-sm outline-none transition"
          :class="'bg-slate-50 dark:bg-slate-950 border-slate-200 dark:border-slate-700 text-slate-900 dark:text-white focus:border-blue-500 dark:bg-slate-800 dark:border-slate-700 dark:text-white dark:focus:border-blue-500'" />
      </div>
      
      <div>
        <label class="block text-[9px] font-black uppercase tracking-widest mb-1.5"
          :class="'text-slate-400 dark:text-slate-600'">To</label>
        <input v-model="localFilters.dateTo" @change="applyFilters" type="date"
          class="w-full border px-3 py-2.5 rounded-xl text-sm outline-none transition"
          :class="'bg-slate-50 dark:bg-slate-950 border-slate-200 dark:border-slate-700 text-slate-900 dark:text-white focus:border-blue-500 dark:bg-slate-800 dark:border-slate-700 dark:text-white dark:focus:border-blue-500'" />
      </div>
      
      <div class="flex gap-2 items-end">
        <div class="flex-1">
          <label class="block text-[9px] font-black uppercase tracking-widest mb-1.5"
            :class="'text-slate-400 dark:text-slate-600'">Sort by</label>
          <select v-model="localFilters.sortBy" @change="applyFilters"
            class="w-full border px-3 py-2.5 rounded-xl text-sm outline-none transition"
            :class="'bg-slate-50 dark:bg-slate-950 border-slate-200 dark:border-slate-700 text-slate-900 dark:text-white focus:border-blue-500 dark:bg-slate-800 dark:border-slate-700 dark:text-white dark:focus:border-blue-500'">
            <option value="createdAt">Date</option>
            <option value="title">Title</option>
            <option value="status">Status</option>
            <option value="priority">Priority</option>
          </select>
        </div>
        <button @click="toggleDescending"
          class="p-2.5 rounded-xl border transition-all mb-0.5"
          :class="'bg-slate-50 dark:bg-slate-950 border-slate-200 dark:border-slate-700 text-slate-500 hover:border-blue-400 dark:bg-slate-800 dark:border-slate-700 dark:text-slate-400 dark:hover:border-blue-500'">
          <ArrowDownIcon v-if="localFilters.descending" class="w-4 h-4" />
          <ArrowUpIcon   v-else                         class="w-4 h-4" />
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';
import {
  SlidersHorizontalIcon, ChevronDownIcon, SearchIcon,
  ArrowUpIcon, ArrowDownIcon
} from 'lucide-vue-next';
import { useTicketsStore } from '../store/tickets';
import { useSettingsStore } from '../store/settings';

const ticketsStore  = useTicketsStore();
const settingsStore = useSettingsStore();

const expanded = ref(true);

const localFilters = reactive({
  title:     '',
  status:     null,
  priority:  null,
  dateFrom: null,
  dateTo: null,
  sortBy:     'createdAt',
  descending: true,
});

const applyFilters = () => {
  ticketsStore.applyFilters({ ...localFilters });
};

let debounceTimer = null;
const debouncedApply = () => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(applyFilters, 400);
};

const toggleDescending = () => {
  localFilters.descending = !localFilters.descending;
  applyFilters();
};
</script>
