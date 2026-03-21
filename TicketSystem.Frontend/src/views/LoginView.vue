<template>
  <div class="min-h-screen flex items-center justify-center px-4 transition-colors"
    :class="settingsStore.isDark ? 'bg-slate-950' : 'bg-slate-100'">

    <div class="w-full max-w-sm space-y-6">

      <div class="flex flex-col items-center text-center gap-3">
        <div class="w-11 h-11 bg-blue-600 rounded-2xl flex items-center justify-center shadow-lg shadow-blue-600/25">
          <TicketIcon class="w-5 h-5 text-white" />
        </div>
        <div>
          <h1 class="text-lg font-black tracking-tight"
            :class="settingsStore.isDark ? 'text-white' : 'text-slate-900'">
            TicketSystem
          </h1>
          <p class="text-[10px] font-bold uppercase tracking-[0.15em] mt-0.5"
            :class="settingsStore.isDark ? 'text-slate-600' : 'text-slate-400'">
            Support Platform
          </p>
        </div>
      </div>

      <div class="rounded-2xl border p-7 transition-colors"
        :class="settingsStore.isDark
          ? 'bg-slate-900 border-slate-800'
          : 'bg-white border-slate-200 shadow-sm'">

        <div class="mb-6">
          <h2 class="text-xl font-black"
            :class="settingsStore.isDark ? 'text-white' : 'text-slate-900'">
            Log in
          </h2>
          <p class="text-xs mt-1"
            :class="settingsStore.isDark ? 'text-slate-500' : 'text-slate-400'">
            Enter your email and password to continue
          </p>
        </div>

        <form @submit.prevent="handleLogin" class="space-y-4">
          <div class="space-y-1.5">
            <label class="text-[10px] font-black uppercase tracking-widest"
              :class="settingsStore.isDark ? 'text-slate-500' : 'text-slate-400'">
              Email address
            </label>
            <input
              v-model="email"
              type="email"
              autocomplete="email"
              @input="formErrors.email = ''"
              placeholder="name@company.com"
              class="w-full border px-4 py-2.5 rounded-xl text-sm outline-none transition-all font-medium"
              :class="formErrors.email
                ? 'border-rose-400 bg-rose-500/5 dark:bg-rose-500/5 text-rose-600 dark:text-rose-400 placeholder-rose-300'
                : settingsStore.isDark
                  ? 'bg-slate-800 border-slate-700 text-white placeholder-slate-600 focus:border-blue-500'
                  : 'bg-slate-50 border-slate-200 text-slate-900 placeholder-slate-400 focus:border-blue-500'" />
            <p v-if="formErrors.email"
              class="text-rose-500 text-[11px] font-semibold flex items-center gap-1">
              <AlertCircleIcon class="w-3 h-3 flex-shrink-0" />
              {{ formErrors.email }}
            </p>
          </div>

          <div class="space-y-1.5">
            <div class="flex items-center justify-between">
              <label class="text-[10px] font-black uppercase tracking-widest"
                :class="settingsStore.isDark ? 'text-slate-500' : 'text-slate-400'">
                Password
              </label>
              <router-link to="/forgot-password"
                class="text-[10px] font-bold text-blue-500 hover:text-blue-400 transition-colors uppercase tracking-wider">
                Forgot yours?
              </router-link>
            </div>
            <input
              v-model="password"
              type="password"
              autocomplete="current-password"
              @input="formErrors.password = ''"
              placeholder="••••••••"
              class="w-full border px-4 py-2.5 rounded-xl text-sm outline-none transition-all font-medium"
              :class="formErrors.password
                ? 'border-rose-400 bg-rose-500/5 dark:bg-rose-500/5 placeholder-rose-300'
                : settingsStore.isDark
                  ? 'bg-slate-800 border-slate-700 text-white placeholder-slate-600 focus:border-blue-500'
                  : 'bg-slate-50 border-slate-200 text-slate-900 placeholder-slate-400 focus:border-blue-500'" />
            <p v-if="formErrors.password"
              class="text-rose-500 text-[11px] font-semibold flex items-center gap-1">
              <AlertCircleIcon class="w-3 h-3 flex-shrink-0" />
              {{ formErrors.password }}
            </p>
          </div>

          <ErrorMessage :message="authStore.error" />

          <button
            type="submit"
            :disabled="authStore.loading"
            class="w-full bg-blue-600 hover:bg-blue-700 active:scale-[0.98] text-white font-black py-2.5 rounded-xl text-xs uppercase tracking-widest transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2 shadow-sm shadow-blue-600/20 mt-2">
            <Loader2Icon v-if="authStore.loading" class="w-4 h-4 animate-spin" />
            <span v-else>Log In</span>
          </button>

          
          <div class="pt-4 space-y-3">
            <div class="flex items-center gap-3">
              <div class="flex-1 h-px" :class="settingsStore.isDark ? 'bg-slate-800' : 'bg-slate-100'"></div>
              <p class="text-[10px] font-bold uppercase tracking-[0.15em] whitespace-nowrap"
                :class="settingsStore.isDark ? 'text-slate-600' : 'text-slate-400'">
                Quick access · Portfolio
              </p>
              <div class="flex-1 h-px" :class="settingsStore.isDark ? 'bg-slate-800' : 'bg-slate-100'"></div>
            </div>
            <div class="grid grid-cols-3 gap-2">
              <button
                type="button"
                @click="fillDemo('admin')"
                class="flex items-center justify-center gap-1.5 border rounded-xl py-2.5 px-2 text-xs font-bold transition-all active:scale-[0.97]"
                :class="settingsStore.isDark
                  ? 'bg-slate-800 border-slate-700 text-slate-300 hover:border-blue-500 hover:text-blue-400'
                  : 'bg-slate-50 border-slate-200 text-slate-600 hover:border-blue-400 hover:text-blue-600'">
                <ShieldCheckIcon class="w-3.5 h-3.5 flex-shrink-0" />
                Admin
              </button>
              <button
                type="button"
                @click="fillDemo('operator')"
                class="flex items-center justify-center gap-1.5 border rounded-xl py-2.5 px-2 text-xs font-bold transition-all active:scale-[0.97]"
                :class="settingsStore.isDark
                  ? 'bg-slate-800 border-slate-700 text-slate-300 hover:border-blue-500 hover:text-blue-400'
                  : 'bg-slate-50 border-slate-200 text-slate-600 hover:border-blue-400 hover:text-blue-600'">
                <UserIcon class="w-3.5 h-3.5 flex-shrink-0" />
                Operator
              </button>
              <button
                type="button"
                @click="fillDemo('user')"
                class="flex items-center justify-center gap-1.5 border rounded-xl py-2.5 px-2 text-xs font-bold transition-all active:scale-[0.97]"
                :class="settingsStore.isDark
                  ? 'bg-slate-800 border-slate-700 text-slate-300 hover:border-blue-500 hover:text-blue-400'
                  : 'bg-slate-50 border-slate-200 text-slate-600 hover:border-blue-400 hover:text-blue-600'">
                <UserCircleIcon class="w-3.5 h-3.5 flex-shrink-0" />
                User
              </button>
            </div>
          </div>
        </form>
      </div>

      <p class="text-center text-xs"
        :class="settingsStore.isDark ? 'text-slate-600' : 'text-slate-400'">
        New to the platform?
        <router-link to="/register"
          class="font-bold text-blue-500 hover:text-blue-400 transition-colors ml-1">
          Create an account
        </router-link>
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '../store/auth';
import { useNotificationStore } from '../store/notifications';
import { useSettingsStore } from '../store/settings';
import { TicketIcon, Loader2Icon, AlertCircleIcon, ShieldCheckIcon, UserIcon, UserCircleIcon } from 'lucide-vue-next';
import ErrorMessage from '../components/ErrorMessage.vue';

const authStore         = useAuthStore();
const notificationStore = useNotificationStore();
const settingsStore     = useSettingsStore();
const router            = useRouter();
const route             = useRoute();

const email    = ref('');
const password = ref('');
const formErrors = ref({ email: '', password: '' });

const DEMO_CREDENTIALS = {
  admin:    { email: 'admin@ticketsystem.com',    password: 'Admin123' },
  operator: { email: 'operador@ticketsystem.com', password: 'Operador123' },
  user:     { email: 'usuario@ticketsystem.com',  password: 'Usuario123' }
};

const fillDemo = (role) => {
  const creds = DEMO_CREDENTIALS[role];
  email.value    = creds.email;
  password.value = creds.password;
  formErrors.value = { email: '', password: '' };
  authStore.error = null;
};

onMounted(() => {
  const params = new URLSearchParams(window.location.search);
  const msg = params.get('mensaje');
  if (msg) authStore.error = decodeURIComponent(msg);

  if (route.query.expired === 'true') {
    notificationStore.error('Your session has expired. Please log in again.');
    router.replace('/');
  }
});

const handleLogin = async () => {
  formErrors.value = { email: '', password: '' };
  let hasError = false;

  if (!email.value) {
    formErrors.value.email = 'Email is required';
    hasError = true;
  }
  if (!password.value) {
    formErrors.value.password = 'Password is required';
    hasError = true;
  }
  if (hasError) return;

  const success = await authStore.login(email.value, password.value);
  if (success) {
    notificationStore.success(`Welcome back, ${authStore.user?.name}!`);
    router.push(authStore.isAdmin || authStore.isOperator ? '/admin' : '/dashboard');
  }
};
</script>
