<template>
  <span :class="badgeClass" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold tracking-wide">
    {{ displayLabel }}
  </span>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  type: [String, Number],
  label: [String, Number]
});

const badgeClass = computed(() => {
  const typeStr = String(props.type || '').toLowerCase().replaceAll(' ', '');
  const types = {
    // Priorities
    low: 'bg-emerald-50 text-emerald-600 border border-emerald-100',
    medium: 'bg-amber-50 dark:bg-amber-900/20 text-amber-700 dark:text-amber-400 border border-amber-200 dark:border-amber-800/40',
    high: 'bg-rose-50 text-rose-600 border border-rose-100',
    critical: 'bg-red-100 text-red-700 border border-red-200 font-bold pulse-subtle',
    
    // Statuses
    pending: 'bg-blue-50 text-blue-600 border border-blue-100',
    inprogress: 'bg-indigo-50 text-indigo-600 border border-indigo-100',
    resolved: 'bg-emerald-50 text-emerald-600 border border-emerald-100',
    closed: 'bg-slate-50 dark:bg-slate-950 text-slate-500 border border-slate-100 dark:border-slate-800/80',
    waitingforuser: 'bg-amber-50 dark:bg-amber-900/20 text-amber-700 dark:text-amber-400 border border-amber-200 dark:border-amber-800/40 sla-pulse',
    waitinguser: 'bg-amber-50 dark:bg-amber-900/20 text-amber-700 dark:text-amber-400 border border-amber-200 dark:border-amber-800/40 sla-pulse',
    
    // Numeric mapping (Legacy/API)
    0: 'bg-blue-50 text-blue-600 border border-blue-100',
    1: 'bg-indigo-50 text-indigo-600 border border-indigo-100',
    2: 'bg-emerald-50 text-emerald-600 border border-emerald-100',
    3: 'bg-slate-50 dark:bg-slate-950 text-slate-500 border border-slate-100 dark:border-slate-800/80',
    4: 'bg-amber-50 dark:bg-amber-900/20 text-amber-700 dark:text-amber-400 border border-amber-200 dark:border-amber-800/40 sla-pulse',
    
    // Spanish mapping for backwards compatibility during migration if needed
    baja: 'bg-emerald-50 text-emerald-600 border border-emerald-100',
    media: 'bg-amber-50 dark:bg-amber-900/20 text-amber-700 dark:text-amber-400 border border-amber-200 dark:border-amber-800/40',
    alta: 'bg-rose-50 text-rose-600 border border-rose-100',
    pendiente: 'bg-blue-50 text-blue-600 border border-blue-100',
    enproceso: 'bg-indigo-50 text-indigo-600 border border-indigo-100',
    resuelto: 'bg-emerald-50 text-emerald-600 border border-emerald-100',
    cerrado: 'bg-slate-50 dark:bg-slate-950 text-slate-500 border border-slate-100 dark:border-slate-800/80',
    esperandousuario: 'bg-amber-50 dark:bg-amber-900/20 text-amber-700 dark:text-amber-400 border border-amber-200 dark:border-amber-800/40 sla-pulse',
  };
  return types[typeStr] || 'bg-slate-500/10 text-slate-500 border border-slate-500/20';
});

const displayLabel = computed(() => {
  const labelStr = String(props.label ?? props.type ?? '').toLowerCase().replaceAll(' ', '');
  const translations = {
    pending: 'Pending',
    inprogress: 'In Progress',
    resolved: 'Resolved',
    closed: 'Closed',
    waitingforuser: 'Waiting for User',
    waitinguser: 'Waiting for User',
    low: 'Low',
    medium: 'Medium',
    high: 'High',
    critical: 'Critical',
    
    // Spanish mapping
    pendiente: 'Pending',
    enproceso: 'In Progress',
    resuelto: 'Resolved',
    cerrado: 'Closed',
    esperandousuario: 'Waiting for User',
    baja: 'Low',
    media: 'Medium',
    alta: 'High',
    
    // Numeric mapping
    0: 'Pending',
    1: 'In Progress',
    2: 'Resolved',
    3: 'Closed',
    4: 'Waiting for User'
  };
  return translations[labelStr] || props.label || props.type;
});
</script>
