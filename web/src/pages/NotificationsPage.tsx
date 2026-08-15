import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/api/client'
import type { NotificationKind } from '@/api/types'
import { useCompanies } from '@/app/contexts'
import { EmptyState, ErrorBox, Loading } from '@/components/Common'
import { formatDate } from '@/lib/format'

const kindLabels: Record<NotificationKind, string> = {
  DeadlineApproaching: 'Son tarih yaklaşıyor',
  NewMatch: 'Yeni eşleşme',
  ScoreChanged: 'Skor değişti',
  RegulationChanged: 'Mevzuat değişti',
  DocumentMissing: 'Eksik belge',
  SystemAlert: 'Sistem uyarısı',
}

const kindClass: Record<NotificationKind, string> = {
  DeadlineApproaching: 'conditional',
  NewMatch: 'eligible',
  ScoreChanged: 'indeterminate',
  RegulationChanged: 'conditional',
  DocumentMissing: 'not-eligible',
  SystemAlert: 'indeterminate',
}

export default function NotificationsPage() {
  const { selectedCompanyId } = useCompanies()
  const queryClient = useQueryClient()

  const { data, isLoading, error } = useQuery({
    queryKey: ['notifications', selectedCompanyId],
    queryFn: () => api.listNotifications({ companyId: selectedCompanyId ?? undefined, pageSize: 50 }),
  })

  const markRead = useMutation({
    mutationFn: (id: string) => api.markNotificationRead(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['notifications', selectedCompanyId] }),
  })

  if (isLoading) return <Loading />
  if (error) return <ErrorBox error={error} />

  return (
    <>
      <div className="page-header">
        <div>
          <h1>Bildirimler</h1>
          <p>Son başvuru uyarıları, yeni eşleşmeler ve skor değişimleri.</p>
        </div>
      </div>

      {!data || data.items.length === 0 ? (
        <EmptyState>Bildirim yok.</EmptyState>
      ) : (
        <div className="card">
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Tür</th>
                  <th>Başlık</th>
                  <th>Tarih</th>
                  <th>Durum</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {data.items.map((notification) => (
                  <tr key={notification.id}>
                    <td>
                      <span className={`badge ${kindClass[notification.kind]}`}>
                        {kindLabels[notification.kind]}
                      </span>
                    </td>
                    <td>
                      <strong>{notification.title}</strong>
                      <div className="muted" style={{ fontSize: 13 }}>
                        {notification.body}
                      </div>
                    </td>
                    <td>{formatDate(notification.createdAt)}</td>
                    <td>{notification.isRead ? 'Okundu' : 'Yeni'}</td>
                    <td>
                      {notification.isRead ? null : (
                        <button
                          type="button"
                          onClick={() => markRead.mutate(notification.id)}
                          disabled={markRead.isPending}
                        >
                          Okundu işaretle
                        </button>
                      )}
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
