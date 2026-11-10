export function parseApiDate(value?: string) {
  if (!value) return null
  const dateOnly = /^\d{4}-\d{2}-\d{2}$/.test(value)
  const date = new Date(dateOnly ? `${value}T00:00:00` : value)
  return Number.isNaN(date.getTime()) ? null : date
}
