import { useEffect, useId, useRef } from 'react'
import { Icon } from '../Icon'

interface ConfirmDialogProps {
  open: boolean
  title: string
  description: string
  confirmLabel?: string
  busy?: boolean
  onConfirm: () => void
  onClose: () => void
}

export function ConfirmDialog({ open, title, description, confirmLabel = 'Confirmar exclusão', busy, onConfirm, onClose }: ConfirmDialogProps) {
  const dialogRef = useRef<HTMLDialogElement>(null)
  const titleId = useId()
  const descriptionId = useId()

  useEffect(() => {
    const dialog = dialogRef.current
    if (!dialog) return
    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) dialog.close()
  }, [open])

  return (
    <dialog ref={dialogRef} className="confirm-dialog" aria-labelledby={titleId} aria-describedby={descriptionId} onCancel={(event) => { event.preventDefault(); onClose() }}>
      <button className="dialog-close" aria-label="Fechar confirmação" onClick={onClose}><Icon name="close" /></button>
      <div className="dialog-danger-icon"><Icon name="trash" size={24} /></div>
      <h2 id={titleId}>{title}</h2>
      <p id={descriptionId}>{description}</p>
      <div className="dialog-actions">
        <button className="secondary-button" onClick={onClose} disabled={busy}>Cancelar</button>
        <button className="danger-button" onClick={onConfirm} disabled={busy}>{busy ? 'Excluindo…' : confirmLabel}</button>
      </div>
    </dialog>
  )
}
