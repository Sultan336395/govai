import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/api/client'
import type { SourceDto } from '@/api/types'
import { EmptyState, ErrorBox, Loading } from '@/components/Common'
import { formatDate } from '@/lib/format'

const statusLabels: Record<SourceDto['lastRunStatus'], string> = {
  Pending: 'Beklemede',
  Running: 'Çalışıyor',
  Succeeded: 'Başarılı',
  Failed: 'Başarısız',
  Skipped: 'Atlandı',
}

const statusClass: Record<SourceDto['lastRunStatus'], string> = {
  Pending: 'indeterminate',
  Running: 'conditional',
  Succeeded: 'eligible',
  Failed: 'not-eligible',
  Skipped: 'indeterminate',
}

export default function SourcesPage() {
  const queryClient = useQueryClient()

  const { data, isLoading, error } = useQuery({
    queryKey: ['sources'],
    queryFn: api.listSources,
  })

  const crawl = useMutation({
    mutationFn: (sourceId: string) => api.triggerCrawl(sourceId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['sources'] }),
  })

  if (isLoading) return <Loading />
  if (error) return <ErrorBox error={error} />

  return (
    <>
      <div className="page-header">
        <div>
          <h1>Veri kaynakları</h1>
          <p>Resmî kurum siteleri ve tarama takvimleri. Üst üste hata alan kaynak otomatik durdurulur.</p>
        </div>
      </div>

      {crawl.error ? <ErrorBox error={crawl.error} /> : null}

      {!data || data.length === 0 ? (
        <EmptyState>Tanımlı kaynak yok.</EmptyState>
      ) : (
        <div className="card">
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Kaynak</th>
                  <th>Takvim</th>
                  <th>Son çalışma</th>
                  <th>Durum</th>
                  <th>Hata sayacı</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {data.map((source) => (
                  <tr key={source.id}>
                    <td>
                      <strong>{source.name}</strong>
                      <div className="muted" style={{ fontSize: 12 }}>
                        <a href={source.baseUrl} target="_blank" rel="noreferrer noopener">
                          {source.baseUrl}
                        </a>
                      </div>
                    </td>
                    <td>
                      <code>{source.cronExpression}</code>
                      <div className="muted" style={{ fontSize: 12 }}>
                        {source.isEnabled ? 'Etkin' : 'Devre dışı'}
                      </div>
                    </td>
                    <td>
                      {formatDate(source.lastRunAt)}
                      {source.lastRunMessage ? (
                        <div className="muted" style={{ fontSize: 12 }}>
                          {source.lastRunMessage}
                        </div>
                      ) : null}
                    </td>
                    <td>
                      <span className={`badge ${statusClass[source.lastRunStatus]}`}>
                        {statusLabels[source.lastRunStatus]}
                      </span>
                    </td>
                    <td>{source.consecutiveFailureCount}</td>
                    <td>
                      <button
                        type="button"
                        onClick={() => crawl.mutate(source.id)}
                        disabled={crawl.isPending}
                      >
                        Şimdi tara
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </>
  )
}
