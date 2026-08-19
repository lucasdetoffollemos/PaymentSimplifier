<script setup lang="ts">
import { ref } from 'vue'

type UserType = 1 | 2

type CreatedUser = {
  id: string
  name: string
  document: string
  email: string
  userType: UserType
  balance: number
}

const apiUrl = 'http://localhost:5049/Users'

const name = ref('')
const document = ref('')
const email = ref('')
const password = ref('')
const userType = ref<UserType>(1)

const isLoading = ref(false)
const errorMessage = ref('')
const createdUser = ref<CreatedUser | null>(null)
let successToastTimeout: ReturnType<typeof setTimeout> | null = null

function onlyNumbers(value: string) {
  return value.replace(/\D/g, '')
}

function formatCpf(value: string) {
  const numbers = onlyNumbers(value).slice(0, 11)

  return numbers
    .replace(/^(\d{3})(\d)/, '$1.$2')
    .replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/^(\d{3})\.(\d{3})\.(\d{3})(\d)/, '$1.$2.$3-$4')
}

function formatCnpj(value: string) {
  const numbers = onlyNumbers(value).slice(0, 14)

  return numbers
    .replace(/^(\d{2})(\d)/, '$1.$2')
    .replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/^(\d{2})\.(\d{3})\.(\d{3})(\d)/, '$1.$2.$3/$4')
    .replace(/^(\d{2})\.(\d{3})\.(\d{3})\/(\d{4})(\d)/, '$1.$2.$3/$4-$5')
}

function formatDocument(value: string) {
  return userType.value === 1 ? formatCpf(value) : formatCnpj(value)
}

function formatDocumentInput() {
  document.value = formatDocument(document.value)
}

async function createUser() {
  isLoading.value = true
  errorMessage.value = ''
  createdUser.value = null

  if (successToastTimeout) {
    clearTimeout(successToastTimeout)
  }

  const requestBody = {
    name: name.value,
    document: onlyNumbers(document.value),
    email: email.value,
    password: password.value,
    userType: userType.value,
  }

  try {
    const response = await fetch(apiUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(requestBody),
    })

    if (!response.ok) {
      const apiError = await response.text()
      throw new Error(apiError || 'Could not create the user.')
    }

    createdUser.value = await response.json()
    
    successToastTimeout = setTimeout(() => {
      createdUser.value = null
      successToastTimeout = null
    }, 5000)

    name.value = ''
    document.value = ''
    email.value = ''
    password.value = ''
    userType.value = 1
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unexpected error.'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <main class="page">
    <section class="hero">
      <p class="eyebrow">Payment Simplifier</p>
      <h1>Create user</h1>
    </section>

    <section class="card">
      <form class="form" @submit.prevent="createUser">
        <label>
          Name
          <input v-model="name" type="text" placeholder="Example: Lucas Silva" required />
        </label>

        <label>
          Document
          <input
            v-model="document"
            type="text"
            :maxlength="userType === 1 ? 14 : 18"
            :placeholder="
              userType === 1 ? 'Example: 154.223.345-65' : 'Example: 12.345.678/0001-99'
            "
            required
            @input="formatDocumentInput"
          />
        </label>

        <label>
          Email
          <input v-model="email" type="email" placeholder="Example: user@email.com" required />
        </label>

        <label>
          Password
          <input v-model="password" type="password" placeholder="Create a password" required />
        </label>

        <label>
          User type
          <select v-model="userType" required @change="formatDocumentInput">
            <option :value="1">Common</option>
            <option :value="2">Merchant</option>
          </select>
        </label>

        <button type="submit" :disabled="isLoading">
          {{ isLoading ? 'Creating...' : 'Create user' }}
        </button>
      </form>

      <p v-if="errorMessage" class="message error">{{ errorMessage }}</p>

      <div v-if="createdUser" class="toast success" role="status" aria-live="polite">
        <strong>User created successfully!</strong>
        <span>ID: {{ createdUser.id }}</span>
        <span>Email: {{ createdUser.email }}</span>
        <span>Balance: {{ createdUser.balance }}</span>
      </div>
    </section>
  </main>
</template>

<style scoped>
:global(*) {
  box-sizing: border-box;
}

:global(body) {
  margin: 0;
  min-width: 320px;
  min-height: 100vh;
  font-family:
    Inter,
    ui-sans-serif,
    system-ui,
    -apple-system,
    BlinkMacSystemFont,
    'Segoe UI',
    sans-serif;
  color: #172033;
  background: linear-gradient(135deg, #edf4ff 0%, #f8fbff 45%, #ecfff7 100%);
}

button,
input,
select {
  font: inherit;
}

.page {
  display: grid;
  grid-template-columns: 1fr minmax(320px, 460px);
  gap: 48px;
  align-items: center;
  width: min(1120px, calc(100% - 32px));
  min-height: 100vh;
  margin: 0 auto;
  padding: 48px 0;
}

.hero {
  max-width: 560px;
}

.eyebrow {
  margin: 0 0 12px;
  font-size: 0.82rem;
  font-weight: 800;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: #157a59;
}

h1 {
  margin: 0;
  font-size: clamp(2.6rem, 7vw, 5.5rem);
  line-height: 0.95;
  letter-spacing: -0.06em;
}

.description {
  margin: 24px 0 0;
  max-width: 460px;
  font-size: 1.15rem;
  line-height: 1.6;
  color: #526076;
}

.card {
  padding: 28px;
  border: 1px solid rgba(23, 32, 51, 0.08);
  border-radius: 28px;
  background: rgba(255, 255, 255, 0.82);
  box-shadow: 0 24px 80px rgba(34, 62, 105, 0.14);
  backdrop-filter: blur(18px);
}

.form {
  display: grid;
  gap: 18px;
}

label {
  display: grid;
  gap: 8px;
  font-size: 0.9rem;
  font-weight: 700;
  color: #303b4f;
}

input,
select {
  width: 100%;
  border: 1px solid #d9e2ef;
  border-radius: 16px;
  padding: 14px 16px;
  color: #172033;
  background: #ffffff;
  outline: none;
  transition:
    border-color 0.2s ease,
    box-shadow 0.2s ease;
}

input:focus,
select:focus {
  border-color: #20b486;
  box-shadow: 0 0 0 4px rgba(32, 180, 134, 0.14);
}

button {
  margin-top: 8px;
  border: 0;
  border-radius: 18px;
  padding: 15px 18px;
  font-weight: 800;
  color: #ffffff;
  background: linear-gradient(135deg, #18a573, #1172e8);
  cursor: pointer;
  transition:
    opacity 0.2s ease,
    transform 0.2s ease;
}

button:hover:not(:disabled) {
  transform: translateY(-1px);
}

button:disabled {
  cursor: not-allowed;
  opacity: 0.65;
}

.message {
  margin: 20px 0 0;
  border-radius: 18px;
  padding: 16px;
  line-height: 1.5;
}

.error {
  color: #8f1d1d;
  background: #fff0f0;
}

.success {
  display: grid;
  gap: 4px;
  color: #13543f;
  background: #edfff8;
}

.toast {
  display: grid;
  gap: 4px;
  margin: 20px 0 0;
  border: 1px solid rgba(19, 84, 63, 0.14);
  border-radius: 18px;
  padding: 16px;
  line-height: 1.5;
  box-shadow: 0 18px 48px rgba(34, 62, 105, 0.18);
}

@media (max-width: 820px) {
  .page {
    grid-template-columns: 1fr;
    gap: 28px;
    align-items: start;
  }

  .card {
    padding: 22px;
  }
}
</style>
