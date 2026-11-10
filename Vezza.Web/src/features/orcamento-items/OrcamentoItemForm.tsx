import { useEffect, useState, type FormEvent } from 'react'
import type { OrcamentoItemInput } from './orcamento-item-types'

interface Props { initialValues: OrcamentoItemInput; busy?: boolean; serverError?: string; submitLabel: string; onSubmit: (values: OrcamentoItemInput) => void; onCancel: () => void }

export function OrcamentoItemForm({ initialValues, busy, serverError, submitLabel, onSubmit, onCancel }: Props) {
  const [values, setValues] = useState(initialValues)
  const [errors, setErrors] = useState<Record<string, string>>({})
  useEffect(() => { setValues(initialValues); setErrors({}) }, [initialValues])
  function submit(event: FormEvent) {
    event.preventDefault()
    const nextErrors: Record<string, string> = {}
    if (!values.descricao.trim()) nextErrors.descricao = 'Descrição é obrigatória.'
    if (!values.valor.trim()) nextErrors.valor = 'Valor é obrigatório.'
    else if (!Number.isFinite(Number(values.valor))) nextErrors.valor = 'Informe um valor válido.'
    setErrors(nextErrors)
    if (Object.keys(nextErrors).length === 0) onSubmit(values)
  }
  return <form className="item-form" onSubmit={submit} noValidate>
    {serverError && <div className="form-error" role="alert">{serverError}</div>}
    <div className="item-form-grid">
      <div className="field item-description-field"><label htmlFor="item-descricao">Descrição<span aria-hidden="true"> *</span></label><input id="item-descricao" value={values.descricao} onChange={(event) => { setValues((current) => ({ ...current, descricao: event.target.value })); setErrors((current) => ({ ...current, descricao: '' })) }} disabled={busy} aria-invalid={Boolean(errors.descricao)} aria-describedby={errors.descricao ? 'item-descricao-error' : undefined} />{errors.descricao && <span className="field-error" id="item-descricao-error">{errors.descricao}</span>}</div>
      <div className="field"><label htmlFor="item-valor">Valor<span aria-hidden="true"> *</span></label><input id="item-valor" type="number" inputMode="decimal" step="0.01" value={values.valor} onChange={(event) => { setValues((current) => ({ ...current, valor: event.target.value })); setErrors((current) => ({ ...current, valor: '' })) }} disabled={busy} aria-invalid={Boolean(errors.valor)} aria-describedby={errors.valor ? 'item-valor-error' : undefined} />{errors.valor && <span className="field-error" id="item-valor-error">{errors.valor}</span>}</div>
    </div>
    <div className="item-form-actions"><button type="button" className="secondary-button" onClick={onCancel} disabled={busy}>Cancelar</button><button type="submit" className="solid-button" disabled={busy}>{busy ? 'Salvando…' : submitLabel}</button></div>
  </form>
}
