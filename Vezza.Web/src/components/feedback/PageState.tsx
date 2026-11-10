export function LoadingState({ label = 'Carregando informações…' }: { label?: string }) {
  return <div className="page-state" role="status"><span className="spinner" />{label}</div>
}

export function ErrorState({ message, onRetry }: { message: string; onRetry?: () => void }) {
  return <div className="page-state error-state" role="alert"><strong>Não foi possível carregar.</strong><span>{message}</span>{onRetry && <button className="secondary-button" onClick={onRetry}>Tentar novamente</button>}</div>
}
