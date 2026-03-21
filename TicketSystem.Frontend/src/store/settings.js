import { defineStore } from 'pinia';

const translations = {
  en: {
    'nav.myTickets': 'My Tickets',
    'nav.support': 'Support & Tracking',
    'nav.settings': 'Settings',
    'nav.profile': 'My Profile',
    'nav.logout': 'Log Out',
    'nav.newTicket': 'New Ticket',
    'nav.reload': 'Reload',
    'dashboard.greeting': 'Hello, {name}!',
    'dashboard.subtitle': 'Manage and track your support tickets',
    'dashboard.avgResponse': 'Avg Response',
    'dashboard.response': 'Response',
    'dashboard.pending': 'Pending',
    'dashboard.inProgress': 'In Progress',
    'dashboard.resolved': 'Resolved',
    'dashboard.waiting': 'Waiting',
    'dashboard.beingHandled': 'Being handled',
    'dashboard.completed': 'Completed',
    'dashboard.search': 'Search...',
    'dashboard.all': 'All',
    'dashboard.tickets': 'tickets',
    'dashboard.emptyTitle': 'Your inbox is empty',
    'dashboard.emptyNoResults': 'No results',
    'dashboard.emptyDesc': "You have no active tickets. Create one if you need support.",
    'dashboard.emptySearch': 'No results for "{query}".',
    'dashboard.createFirst': 'Create my first ticket',
    'dashboard.page': 'Page',
    'dashboard.of': 'of',
    'dashboard.total': 'total',
    'ticket.new': 'New Ticket',
    'ticket.newDesc': 'Describe your problem in detail',
    'ticket.subject': 'Subject',
    'ticket.subjectPH': 'E.g.: Payment processing error',
    'ticket.description': 'Description',
    'ticket.descPH': "What's going wrong exactly?",
    'ticket.cancel': 'Cancel',
    'ticket.send': 'Submit Request',
    'ticket.sending': 'Sending...',
    'ticket.low': 'Low',
    'ticket.medium': 'Medium',
    'ticket.urgent': 'Urgent',
    'ticket.errSubject': 'Subject must be at least 5 characters.',
    'ticket.errDesc': 'Description must be at least 10 characters.',
    'ticket.successSent': 'Ticket submitted!',
    'ticket.errSend': 'Error submitting ticket.',
    'chat.title': 'Support Chat',
    'chat.empty': 'No activity yet',
    'chat.placeholder': 'Write a message...',
    'chat.internal': 'Internal note...',
    'chat.internalLabel': 'Internal Note',
    'chat.hint': 'Enter to send · Shift+Enter for new line',
    'settings.title': 'Settings',
    'settings.appearance': 'Appearance Mode',
    'settings.light': 'Light',
    'settings.dark': 'Dark',
    'settings.system': 'System',
    'settings.notifications': 'Notifications',
    'settings.notifDesc': 'Real-time alerts',
    'settings.language': 'Language',
    'settings.security': 'Security',
    'settings.currentPass': 'Current Password',
    'settings.newPass': 'New Password',
    'settings.confirmPass': 'Confirm Password',
    'settings.updatePass': 'Update Password',
    'settings.saving': 'Saving...',
    'settings.passMin': 'At least 6 characters',
    'settings.passRepeat': 'Repeat new password',
    'settings.passSuccess': 'Password updated successfully!',
    'settings.errFields': 'Please fill in all fields.',
    'settings.errMin': 'New password must be at least 6 characters.',
    'settings.errMatch': "Passwords don't match.",
    'settings.errUpdate': 'Error updating password.',
    'login.title': 'Sign in',
    'login.subtitle': 'Enter your email and password to continue',
    'login.email': 'Email address',
    'login.emailPH': 'name@company.com',
    'login.password': 'Password',
    'login.forgot': 'Forgot yours?',
    'login.submit': 'Sign In',
    'login.newUser': 'New to the platform?',
    'login.register': 'Create an account',
    'login.errEmail': 'Email address is required',
    'login.errPassword': 'Password is required',
    'forgot.title': 'Recover Access',
    'forgot.subtitle': 'Enter your email and we will send you instructions',
    'forgot.email': 'Email Address',
    'forgot.emailPH': 'you@email.com',
    'forgot.submit': 'Send Instructions',
    'forgot.cancel': 'Cancel',
    'forgot.back': 'Back to Login',
  }
};

export const useSettingsStore = defineStore('settings', {
  state: () => ({
    themeMode: localStorage.getItem('themeMode') || 'light',
    themeColor: localStorage.getItem('themeColor') || 'blue',
    uiDensity: localStorage.getItem('uiDensity') || 'comfortable',
    language: 'en',
    notificationsEnabled: localStorage.getItem('notificationsEnabled') !== 'false',
  }),
  getters: {
    isDark: (state) => state.themeMode === 'dark',
    themeClasses: (state) => {
      const colors = {
        blue: 'from-blue-500 to-blue-700',
        indigo: 'from-indigo-500 to-indigo-700',
        emerald: 'from-emerald-500 to-emerald-700',
        rose: 'from-rose-500 to-rose-700',
        violet: 'from-violet-500 to-violet-700',
      };
      return colors[state.themeColor] || colors.blue;
    },
    accentColor: (state) => {
      const colors = {
        blue: '#3b82f6', indigo: '#6366f1', emerald: '#10b981',
        rose: '#f43f5e', violet: '#8b5cf6',
      };
      return colors[state.themeColor] || colors.blue;
    },
    t: (state) => (key, vars = {}) => {
      const dict = translations.en;
      let text = dict[key] ?? key;
      Object.entries(vars).forEach(([k, v]) => {
        text = text.replace(`{${k}}`, v);
      });
      return text;
    },
  },
  actions: {
    setThemeMode(mode) {
      this.themeMode = mode;
      localStorage.setItem('themeMode', mode);
      this.applyTheme();
    },
    toggleThemeMode() {
      this.setThemeMode(this.themeMode === 'light' ? 'dark' : 'light');
    },
    setThemeColor(color) {
      this.themeColor = color;
      localStorage.setItem('themeColor', color);
      this.applyAccentColor();
    },
    setUIDensity(density) {
      this.uiDensity = density;
      localStorage.setItem('uiDensity', density);
      document.documentElement.setAttribute('data-density', density);
    },
    setLanguage(lang) {
      this.language = 'en';
      localStorage.setItem('language', 'en');
    },
    toggleNotifications() {
      this.notificationsEnabled = !this.notificationsEnabled;
      localStorage.setItem('notificationsEnabled', this.notificationsEnabled);
    },
    applyAccentColor() {
      const colors = {
        blue: '59 130 246', indigo: '99 102 241', emerald: '16 185 129',
        rose: '244 63 94', violet: '139 92 246',
      };
      document.documentElement.style.setProperty('--accent-primary', colors[this.themeColor] || colors.blue);
    },
    applyTheme() {
      const isDark = this.themeMode === 'dark';
      document.documentElement.classList.toggle('dark', isDark);
      document.documentElement.setAttribute('data-theme', this.themeMode);
    },
    initSettings() {
      this.applyTheme();
      this.applyAccentColor();
      document.documentElement.setAttribute('data-density', this.uiDensity);
    }
  }
});
