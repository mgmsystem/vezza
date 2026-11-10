export function toCents(value: number | undefined) {
  return Number.isFinite(value) ? Math.round((value ?? 0) * 100) : 0
}
export function sumItemCents(values: Array<number | undefined>) {
  return values.reduce<number>((total, value) => total + toCents(value), 0)
}
