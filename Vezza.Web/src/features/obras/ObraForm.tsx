import { useState, type ChangeEvent, type FormEvent } from 'react'
import type { Cliente } from '../clientes/cliente-types'
import type { ObraInput } from './obra-types'

interface Props {
  initialValues: ObraInput
  clientes: Cliente[]
  submitLabel: string
  busy?: boolean
  serverError?: string
  onSubmit: (value: ObraInput) => void
  onCancel: () => void
}

export function ObraForm({ initialValues, clientes, submitLabel, busy, serverError, onSubmit, onCancel }: Props) {
  const [values, setValues] = useState(initialValues)
  const [errors, setErrors] = useState<Record<string, string>>({})

  function change(event: ChangeEvent<HTMLInputElement | HTMLSelectElement>) {
    const { name, value } = event.target
    setValues((current) => ({ ...current, [name]: name === 'clienteId' ? Number(value) : name === 'estado' ? value.toUpperCase().slice(0, 2) : value }))
    if (errors[name]) setErrors((current) => ({ ...current, [name]: '' }))
  }

  function submit(event: FormEvent) {
    event.preventDefault()
    const next: Record<string, string> = {}
    if (!values.clienteId || !clientes.some((item) => item.clienteId === values.clienteId)) next.clienteId = 'Selecione um cliente válido.'
    if (!values.descricao.trim()) next.descricao = 'Descrição é obrigatória.'
    else if (values.descricao.trim().length < 3) next.descricao = 'Informe pelo menos 3 caracteres.'
    setErrors(next)
    if (Object.keys(next).length === 0) onSubmit(values)
  }

  const field = (name: Exclude<keyof ObraInput, 'clienteId'>, label: string, options?: { wide?: boolean; maxLength?: number; placeholder?: string }) => (
    <div className={`field ${options?.wide ? 'field-wide' : ''}`}><label htmlFor={name}>{label}{name === 'descricao' && <span aria-hidden="true"> *</span>}</label><input id={name} name={name} value={values[name]} onChange={change} maxLength={options?.maxLength} placeholder={options?.placeholder} disabled={busy} aria-invalid={Boolean(errors[name])} aria-describedby={errors[name] ? `${name}-error` : undefined} />{errors[name] && <span className="field-error" id={`${name}-error`}>{errors[name]}</span>}</div>
  )

  return <form className="entity-form" onSubmit={submit} noValidate>
    {serverError && <div className="form-error" role="alert">{serverError}</div>}
    <section className="form-section"><div className="form-section-heading"><span>01</span><div><h2>Identificação</h2><p>Vincule a obra ao cliente responsável.</p></div></div><div className="form-grid">
      <div className="field field-wide"><label htmlFor="clienteId">Cliente vinculado <span aria-hidden="true">*</span></label><select id="clienteId" name="clienteId" value={values.clienteId || ''} onChange={change} disabled={busy} aria-invalid={Boolean(errors.clienteId)} aria-describedby={errors.clienteId ? 'clienteId-error' : undefined}><option value="">Selecione um cliente</option>{clientes.map((cliente) => <option key={cliente.clienteId} value={cliente.clienteId}>{cliente.razaoSocial} — {cliente.cnpjCpf}</option>)}</select>{errors.clienteId && <span className="field-error" id="clienteId-error">{errors.clienteId}</span>}</div>
      {field('descricao', 'Descrição da obra', { wide: true, placeholder: 'Ex.: Reforma da unidade central' })}
    </div></section>
    <section className="form-section"><div className="form-section-heading"><span>02</span><div><h2>Endereço</h2><p>Local onde a obra será executada.</p></div></div><div className="form-grid">
      {field('cep', 'CEP', { placeholder: '00000-000' })}{field('logradouro', 'Logradouro', { wide: true })}{field('numero', 'Número')}{field('complemento', 'Complemento')}{field('bairro', 'Bairro')}{field('cidade', 'Cidade', { wide: true })}{field('estado', 'Estado', { maxLength: 2, placeholder: 'UF' })}
    </div></section>
    <div className="form-actions"><button type="button" className="secondary-button" onClick={onCancel} disabled={busy}>Cancelar</button><button className="solid-button" type="submit" disabled={busy}>{busy ? 'Salvando…' : submitLabel}</button></div>
  </form>
}
