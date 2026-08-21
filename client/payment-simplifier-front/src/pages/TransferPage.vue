<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'

type UserType = 1 | 2

type User = {
  id: string
  name: string
  document: string
  email: string
  userType: UserType
  balance: number
}

const usersApiUrl = 'http://localhost:5049/Users'
const transfersApiUrl = 'http://localhost:5049/Transfers'

const users = ref<User[]>([])
const usersLoading = ref(false)
const usersErrorMessage = ref('')

const payerId = ref('')
const payeeId = ref('')
const transferAmount = ref('')
const payerPassword = ref('')
const isTransferring = ref(false)
const transferErrorMessage = ref('')
const transferResultMessage = ref('')
let transferToastTimeout: ReturnType<typeof setTimeout> | null = null

const payer = computed(() => users.value.find((user) => user.id === payerId.value) ?? null)
const payee = computed(() => users.value.find((user) => user.id === payeeId.value) ?? null)

const availablePayees = computed(() => users.value.filter((user) => user.id !== payerId.value))

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

function formatDocument(value: string, type: UserType) {
  return type === 1 ? formatCpf(value) : formatCnpj(value)
}

function formatMoney(value: number) {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(value)
}

function getUserTypeLabel(type: UserType) {
  return type === 1 ? 'Common' : 'Merchant'
}

async function fetchUsers() {
  usersLoading.value = true
  usersErrorMessage.value = ''

  try {
    const response = await fetch(usersApiUrl)

    if (!response.ok) {
      const apiError = await response.text()
      throw new Error(apiError || 'Could not load users.')
    }

    users.value = await response.json()
  } catch (error) {
    usersErrorMessage.value = error instanceof Error ? error.message : 'Unexpected error.'
  } finally {
    usersLoading.value = false
  }
}

function validateTransfer(amount: number) {
  if (!payerId.value) {
    throw new Error('Choose a payer.')
  }

  if (!payeeId.value) {
    throw new Error('Choose a payee.')
  }

  if (payerId.value === payeeId.value) {
    throw new Error('Payer and payee must be different users.')
  }

  if (!Number.isFinite(amount) || amount <= 0) {
    throw new Error('Enter a valid transfer amount.')
  }

  if (!payerPassword.value.trim()) {
    throw new Error('Enter the payer password.')
  }
}

async function transferMoney() {
  isTransferring.value = true
  transferErrorMessage.value = ''
  transferResultMessage.value = ''

  if (transferToastTimeout) {
    clearTimeout(transferToastTimeout)
  }

  const value = Number(transferAmount.value)

  try {
    validateTransfer(value)

    const response = await fetch(transfersApiUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        payerId: payerId.value,
        payeeId: payeeId.value,
        value,
        password: payerPassword.value,
      }),
    })

    const responseText = await response.text()

    if (!response.ok) {
      throw new Error(responseText || 'Could not transfer money.')
    }

    transferResultMessage.value = responseText || 'Transfer completed successfully.'

    transferToastTimeout = setTimeout(() => {
      transferResultMessage.value = ''
      transferToastTimeout = null
    }, 5000)

    await fetchUsers()
    transferAmount.value = ''
    payerPassword.value = ''
  } catch (error) {
    transferErrorMessage.value = error instanceof Error ? error.message : 'Unexpected error.'
  } finally {
    isTransferring.value = false
  }
}

onMounted(fetchUsers)
</script>

<template>
  <main class="page">
    <section class="hero">
      <p class="eyebrow">Payment Simplifier</p>
      <h1>Transfer money</h1>
      <p class="description">
        Move money from a common payer to another user after confirming the payer password.
      </p>
    </section>

    <section class="card transfer-card">
      <form class="form" @submit.prevent="transferMoney">
        <label>
          Payer
          <select v-model="payerId" required :disabled="usersLoading || users.length === 0">
            <option value="" disabled>
              {{ usersLoading ? 'Loading users...' : 'Choose payer' }}
            </option>
            <option v-for="user in users" :key="user.id" :value="user.id">
              {{ user.name }} - {{ user.email }}
            </option>
          </select>
        </label>

        <div v-if="payer" class="user-summary payer-summary">
          <span>Payer: {{ getUserTypeLabel(payer.userType) }}</span>
          <strong>{{ formatMoney(payer.balance) }}</strong>
          <small>{{ formatDocument(payer.document, payer.userType) }}</small>
        </div>

        <label>
          Payee
          <select
            v-model="payeeId"
            required
            :disabled="usersLoading || availablePayees.length === 0"
          >
            <option value="" disabled>
              {{ usersLoading ? 'Loading users...' : 'Choose payee' }}
            </option>
            <option v-for="user in availablePayees" :key="user.id" :value="user.id">
              {{ user.name }} - {{ user.email }}
            </option>
          </select>
        </label>

        <div v-if="payee" class="user-summary payee-summary">
          <span>Payee: {{ getUserTypeLabel(payee.userType) }}</span>
          <strong>{{ formatMoney(payee.balance) }}</strong>
          <small>{{ formatDocument(payee.document, payee.userType) }}</small>
        </div>

        <label>
          Amount
          <input
            v-model="transferAmount"
            type="number"
            min="0.01"
            step="0.01"
            placeholder="Example: 100.00"
            required
          />
        </label>

        <label>
          Payer password
          <input
            v-model="payerPassword"
            type="password"
            placeholder="Password for selected payer"
            required
          />
        </label>

        <button class="primary-button" type="submit" :disabled="isTransferring || users.length < 2">
          {{ isTransferring ? 'Transferring...' : 'Transfer money' }}
        </button>
      </form>

      <p v-if="usersErrorMessage" class="message error">{{ usersErrorMessage }}</p>
      <p v-if="transferErrorMessage" class="message error">{{ transferErrorMessage }}</p>

      <div v-if="transferResultMessage" class="toast success" role="status" aria-live="polite">
        <strong>Transfer completed!</strong>
        <span>{{ transferResultMessage }}</span>
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
  background:
    radial-gradient(circle at top left, #e8f2ff 0, transparent 36%),
    linear-gradient(135deg, #f8fbff 0%, #edf7ff 48%, #f0fff8 100%);
}

button,
input,
select {
  font: inherit;
}

.page {
  display: grid;
  grid-template-columns: 1fr minmax(320px, 500px);
  gap: 48px;
  align-items: center;
  width: min(1120px, calc(100% - 32px));
  min-height: 100vh;
  margin: 0 auto;
  padding: 80px 0 48px;
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
  background: rgba(255, 255, 255, 0.84);
  box-shadow: 0 24px 80px rgba(34, 62, 105, 0.14);
  backdrop-filter: blur(18px);
}

.transfer-card {
  border-color: rgba(17, 114, 232, 0.18);
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
  border-color: #1172e8;
  box-shadow: 0 0 0 4px rgba(17, 114, 232, 0.14);
}

.primary-button {
  margin-top: 8px;
  border: 0;
  border-radius: 18px;
  padding: 15px 18px;
  font-weight: 800;
  color: #ffffff;
  background: linear-gradient(135deg, #1172e8, #18a573);
  cursor: pointer;
  transition:
    opacity 0.2s ease,
    transform 0.2s ease;
}

.primary-button:hover:not(:disabled) {
  transform: translateY(-1px);
}

.primary-button:disabled {
  cursor: not-allowed;
  opacity: 0.65;
}

.user-summary {
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 4px 12px;
  border-radius: 18px;
  padding: 16px;
}

.payer-summary {
  border: 1px solid rgba(17, 114, 232, 0.18);
  color: #154a8f;
  background: #edf5ff;
}

.payee-summary {
  border: 1px solid rgba(32, 180, 134, 0.18);
  color: #13543f;
  background: #edfff8;
}

.user-summary strong {
  font-size: 1.2rem;
}

.user-summary small {
  grid-column: 1 / -1;
  color: #526076;
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
    padding-top: 28px;
  }

  .card {
    padding: 22px;
  }
}
</style>
