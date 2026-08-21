<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <header class="app-header">
    <nav class="app-nav" aria-label="Main navigation">
      <RouterLink to="/">Users</RouterLink>
      <RouterLink to="/transfer">Transfer</RouterLink>
    </nav>
  </header>

  <RouterView />
</template>

<style scoped>
.app-header {
  position: fixed;
  top: 20px;
  right: 24px;
  z-index: 10;
}

.app-nav {
  display: inline-flex;
  gap: 8px;
  border: 1px solid rgba(23, 32, 51, 0.08);
  border-radius: 999px;
  padding: 6px;
  background: rgba(255, 255, 255, 0.78);
  box-shadow: 0 16px 48px rgba(34, 62, 105, 0.12);
  backdrop-filter: blur(18px);
}

.app-nav a {
  border-radius: 999px;
  padding: 10px 15px;
  font-size: 0.9rem;
  font-weight: 800;
  color: #526076;
  text-decoration: none;
}

.app-nav a.router-link-active {
  color: #ffffff;
  background: #172033;
}

@media (max-width: 820px) {
  .app-header {
    position: static;
    display: flex;
    justify-content: center;
    padding-top: 20px;
  }
}
</style>
