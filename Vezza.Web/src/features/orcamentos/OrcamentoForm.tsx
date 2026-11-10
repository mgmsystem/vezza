import { useMemo, useState, type ChangeEvent, type FormEvent } from 'react'
import type { Cliente } from '../clientes/cliente-types'
import type { Obra } from '../obras/obra-types'
import type { OrcamentoInput, OrcamentoPayload } from './orcamento-types'

interface Props {
  initialValues: OrcamentoInput
  obras: Obra[]
  clientes: Cliente[]
  submitLabel: string
  busy?: boolean
  serverError?: string
  onSubmit: (value: OrcamentoPayload) => void
  onCancel: () => void
}

export function OrcamentoForm({ initialValues, obras, clientes, submitLabel, busy, serverError, onSubmit, onCancel }: Props) {
  const [values, setValues] = useState(initialValues)
  const [errors, setErrors] = useState<Record<string, string>>({})
  const clienteNames = useMemo(() => new Map(clientes.map((item) => [item.clienteId, item.razaoSocial])), [clientes])

  function change(event: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) {
    const { name, value } = event.target
    setValues((current) => ({ ...current, [name]: name === 'obraId' ? Number(value) : value }))
    if (errors[name]) setErrors((current) => ({ ...current, [name]: '' }))
  }

  function submit(event: FormEvent) {
    event.preventDefault()
    const next: Record<string, string> = {}
    if (!values.obraId || !obras.some((item) => item.obraId === values.obraId)) next.obraId = 'Selecione uma obra válida.'
    if (!values.data) next.data = 'Data é obrigatória.'
    if (!values.objetoProposta.trim()) next.objetoProposta = 'Objeto da proposta é obrigatório.'
    if (values.valor === '' || !Number.isFinite(Number(values.valor))) next.valor = 'Valor do orçamento é obrigatório.'
    setErrors(next)
    if (Object.keys(next).length > 0) return
    onSubmit({
      obraId: values.obraId, data: values.data, objetoProposta: values.objetoProposta.trim(), valor: Number(values.valor),
      condPagamento: values.condPagamento, prazos: values.prazos, comentarios: values.comentarios,
      notasGerais: values.notasGerais, situacao: values.situacao,
      ...(values.inicioObra ? { inicioObra: values.inicioObra } : {}),
      ...(values.terminoObra ? { terminoObra: values.terminoObra } : {}),
    })
  }

  return <form className="entity-form" onSubmit={submit} noValidate>
    {serverError && <div className="form-error" role="alert">{serverError}</div>}
    <section className="form-section"><div className="form-section-heading"><span>01</span><div><h2>Identificação</h2><p>Dados principais e vínculo com a obra.</p></div></div><div className="form-grid">
      <div className="field field-wide"><label htmlFor="obraId">Obra vinculada <span aria-hidden="true">*</span></label><select id="obraId" name="obraId" value={values.obraId || ''} onChange={change} disabled={busy} aria-invalid={Boolean(errors.obraId)} aria-describedby={errors.obraId ? 'obraId-error' : undefined}><option value="">Selecione uma obra</option>{obras.map((obra) => <option key={obra.obraId} value={obra.obraId}>{obra.descricao} — {clienteNames.get(obra.clienteId) ?? `Cliente #${obra.clienteId}`}</option>)}</select>{errors.obraId && <span className="field-error" id="obraId-error">{errors.obraId}</span>}</div>
      <div className="field"><label htmlFor="data">Data <span aria-hidden="true">*</span></label><input id="data" name="data" type="date" value={values.data} onChange={change} disabled={busy} aria-invalid={Boolean(errors.data)} aria-describedby={errors.data ? 'data-error' : undefined} />{errors.data && <span className="field-error" id="data-error">{errors.data}</span>}</div>
      <div className="field field-wide"><label htmlFor="objetoProposta">Objeto da proposta <span aria-hidden="true">*</span></label><textarea id="objetoProposta" name="objetoProposta" rows={3} value={values.objetoProposta} onChange={change} disabled={busy} aria-invalid={Boolean(errors.objetoProposta)} aria-describedby={errors.objetoProposta ? 'objetoProposta-error' : undefined} placeholder="Descreva o objetivo principal do orçamento" />{errors.objetoProposta && <span className="field-error" id="objetoProposta-error">{errors.objetoProposta}</span>}</div>
      <div className="field"><label htmlFor="situacao">Situação</label><input id="situacao" name="situacao" value={values.situacao} onChange={change} disabled={busy} placeholder="Texto livre" /></div>
    </div></section>
    <section className="form-section"><div className="form-section-heading"><span>02</span><div><h2>Valores e condições</h2><p>Informações comerciais da proposta.</p></div></div><div className="form-grid">
      <div className="field"><label htmlFor="valor">Valor do orçamento <span aria-hidden="true">*</span></label><input id="valor" name="valor" type="number" step="0.01" inputMode="decimal" value={values.valor} onChange={change} disabled={busy} aria-invalid={Boolean(errors.valor)} aria-describedby={errors.valor ? 'valor-error' : undefined} placeholder="0,00" />{errors.valor && <span className="field-error" id="valor-error">{errors.valor}</span>}</div>
      <div className="field field-wide"><label htmlFor="condPagamento">Condição de pagamento</label><input id="condPagamento" name="condPagamento" value={values.condPagamento} onChange={change} disabled={busy} /></div>
      <div className="field field-wide"><label htmlFor="prazos">Prazos</label><textarea id="prazos" name="prazos" rows={2} value={values.prazos} onChange={change} disabled={busy} /></div>
    </div></section>
    <section className="form-section"><div className="form-section-heading"><span>03</span><div><h2>Período</h2><p>Datas previstas para execução da obra.</p></div></div><div className="form-grid">
      <div className="field"><label htmlFor="inicioObra">Início da obra</label><input id="inicioObra" name="inicioObra" type="date" value={values.inicioObra} onChange={change} disabled={busy} /></div>
      <div className="field"><label htmlFor="terminoObra">Término da obra</label><input id="terminoObra" name="terminoObra" type="date" value={values.terminoObra} onChange={change} disabled={busy} /></div>
    </div></section>
    <section className="form-section"><div className="form-section-heading"><span>04</span><div><h2>Observações</h2><p>Contexto complementar para a proposta.</p></div></div><div className="form-grid">
      <div className="field field-wide"><label htmlFor="comentarios">Comentários</label><textarea id="comentarios" name="comentarios" rows={4} value={values.comentarios} onChange={change} disabled={busy} /></div>
      <div className="field field-wide"><label htmlFor="notasGerais">Notas gerais</label><textarea id="notasGerais" name="notasGerais" rows={4} value={values.notasGerais} onChange={change} disabled={busy} /></div>
    </div></section>
    <div className="form-actions"><button type="button" className="secondary-button" onClick={onCancel} disabled={busy}>Cancelar</button><button className="solid-button" type="submit" disabled={busy}>{busy ? 'Salvando…' : submitLabel}</button></div>
  </form>
}
