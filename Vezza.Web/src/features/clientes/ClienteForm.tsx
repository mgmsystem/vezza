import { useState, type ChangeEvent, type FormEvent } from 'react'
import type { ClienteInput } from './cliente-types'

interface ClienteFormProps {
  initialValues: ClienteInput
  submitLabel: string
  busy?: boolean
  serverError?: string
  onSubmit: (values: ClienteInput) => void
  onCancel: () => void
}

export function ClienteForm({ initialValues, submitLabel, busy, serverError, onSubmit, onCancel }: ClienteFormProps) {
  const [values, setValues] = useState(initialValues)
  const [errors, setErrors] = useState<Record<string, string>>({})

  function change(event: ChangeEvent<HTMLInputElement>) {
    const { name, value } = event.target
    setValues((current) => ({ ...current, [name]: name === 'estado' ? value.toUpperCase().slice(0, 2) : value }))
    if (errors[name]) setErrors((current) => ({ ...current, [name]: '' }))
  }

  function submit(event: FormEvent) {
    event.preventDefault()
    const nextErrors: Record<string, string> = {}
    if (!values.razaoSocial.trim()) nextErrors.razaoSocial = 'Razão social é obrigatória.'
    else if (values.razaoSocial.trim().length < 3) nextErrors.razaoSocial = 'Informe pelo menos 3 caracteres.'
    if (!values.cnpjCpf.trim()) nextErrors.cnpjCpf = 'CPF/CNPJ é obrigatório.'
    setErrors(nextErrors)
    if (Object.keys(nextErrors).length === 0) onSubmit(values)
  }

  const field = (name: keyof ClienteInput, label: string, options?: { placeholder?: string; required?: boolean; wide?: boolean; maxLength?: number }) => (
    <div className={`field ${options?.wide ? 'field-wide' : ''}`}>
      <label htmlFor={name}>{label}{options?.required && <span aria-hidden="true"> *</span>}</label>
      <input id={name} name={name} value={values[name]} onChange={change} placeholder={options?.placeholder} maxLength={options?.maxLength} disabled={busy} aria-invalid={Boolean(errors[name])} aria-describedby={errors[name] ? `${name}-error` : undefined} />
      {errors[name] && <span className="field-error" id={`${name}-error`}>{errors[name]}</span>}
    </div>
  )

  return (
    <form className="entity-form" onSubmit={submit} noValidate>
      {serverError && <div className="form-error" role="alert">{serverError}</div>}
      <section className="form-section">
        <div className="form-section-heading"><span>01</span><div><h2>Identificação</h2><p>Dados principais do cliente.</p></div></div>
        <div className="form-grid">{field('razaoSocial', 'Razão social', { required: true, wide: true, placeholder: 'Nome ou razão social' })}{field('cnpjCpf', 'CPF/CNPJ', { required: true, placeholder: 'Documento do cliente' })}</div>
      </section>
      <section className="form-section">
        <div className="form-section-heading"><span>02</span><div><h2>Contato</h2><p>Canal principal para comunicação.</p></div></div>
        <div className="form-grid">{field('telefone', 'Telefone', { placeholder: '(00) 00000-0000' })}</div>
      </section>
      <section className="form-section">
        <div className="form-section-heading"><span>03</span><div><h2>Endereço</h2><p>Localização cadastral ou comercial.</p></div></div>
        <div className="form-grid">
          {field('cep', 'CEP', { placeholder: '00000-000' })}{field('logradouro', 'Logradouro', { wide: true, placeholder: 'Rua, avenida ou estrada' })}
          {field('numero', 'Número')}{field('complemento', 'Complemento')}{field('bairro', 'Bairro')}
          {field('cidade', 'Cidade', { wide: true })}{field('estado', 'Estado', { maxLength: 2, placeholder: 'UF' })}
        </div>
      </section>
      <div className="form-actions"><button type="button" className="secondary-button" onClick={onCancel} disabled={busy}>Cancelar</button><button className="solid-button" type="submit" disabled={busy}>{busy ? 'Salvando…' : submitLabel}</button></div>
    </form>
  )
}
