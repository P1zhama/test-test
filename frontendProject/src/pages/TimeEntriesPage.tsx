import { useState } from "react";
import {
  useClosedPeriods,
  useDeleteTimeEntry,
  useEmployees,
  useProjects,
  useTimeEntries,
  useTogglePeriod
} from "../api/hooks";
import type { TimeEntry } from "../api/types";
import { ErrorBanner } from "../components/ErrorBanner";
import { MonthPicker } from "../components/MonthPicker";
import { formatDate, formatHours, formatMoney, monthValue } from "../format";
import { TimeEntryDialog } from "./TimeEntryDialog";

const PAGE_SIZE = 20;

interface Props {
  year: number;
  month: number;
  onMonthChange: (year: number, month: number) => void;
}

export function TimeEntriesPage({ year, month, onMonthChange }: Props) {
  const [employeeId, setEmployeeId] = useState("");
  const [projectId, setProjectId] = useState("");
  const [page, setPage] = useState(1);
  const [dialogEntry, setDialogEntry] = useState<TimeEntry | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [actionError, setActionError] = useState<unknown>(null);

  const employees = useEmployees();
  const projects = useProjects();
  const periods = useClosedPeriods();
  const entries = useTimeEntries({ year, month, employeeId, projectId, page, pageSize: PAGE_SIZE });
  const remove = useDeleteTimeEntry();
  const togglePeriod = useTogglePeriod();

  const isClosed = (periods.data ?? []).some((p) => p.year === year && p.month === month);
  const result = entries.data;

  const resetPage = (apply: () => void) => {
    apply();
    setPage(1);
  };

  const openCreate = () => {
    setDialogEntry(null);
    setDialogOpen(true);
  };

  const openEdit = (entry: TimeEntry) => {
    setDialogEntry(entry);
    setDialogOpen(true);
  };

  const onDelete = async (entry: TimeEntry) => {
    if (!confirm(`Удалить запись от ${formatDate(entry.date)}?`)) return;
    setActionError(null);
    try {
      await remove.mutateAsync(entry.id);
    } catch (error) {
      setActionError(error);
    }
  };

  const onTogglePeriod = async () => {
    setActionError(null);
    try {
      await togglePeriod.mutateAsync({ year, month, close: !isClosed });
    } catch (error) {
      setActionError(error);
    }
  };

  return (
    <section>
      <div className="toolbar">
        <MonthPicker year={year} month={month} onChange={(y, m) => resetPage(() => onMonthChange(y, m))} />

        <label>
          Сотрудник
          <select value={employeeId} onChange={(e) => resetPage(() => setEmployeeId(e.target.value))}>
            <option value="">Все сотрудники</option>
            {(employees.data ?? []).map((employee) => (
              <option key={employee.id} value={employee.id}>
                {employee.fullName}
              </option>
            ))}
          </select>
        </label>

        <label>
          Проект
          <select value={projectId} onChange={(e) => resetPage(() => setProjectId(e.target.value))}>
            <option value="">Все проекты</option>
            {(projects.data ?? []).map((project) => (
              <option key={project.id} value={project.id}>
                {project.code} — {project.name}
              </option>
            ))}
          </select>
        </label>

        <div className="toolbar-actions">
          <button type="button" onClick={openCreate} disabled={isClosed}>
            Добавить запись
          </button>
          <button type="button" className="secondary" onClick={onTogglePeriod}>
            {isClosed ? "Открыть месяц" : "Закрыть месяц"}
          </button>
        </div>
      </div>

      {isClosed && (
        <div className="banner warning">
          Период {monthValue(year, month)} закрыт бухгалтерией: записи только для просмотра.
        </div>
      )}

      <ErrorBanner error={actionError} />
      <ErrorBanner error={entries.error} />

      {entries.isLoading && <div className="muted">Загрузка…</div>}

      <table>
        <thead>
          <tr>
            <th>Дата</th>
            <th>Сотрудник</th>
            <th>Проект</th>
            <th className="num">Часы</th>
            <th className="num">Ставка</th>
            <th className="num">Стоимость</th>
            <th>Комментарий</th>
            <th></th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {(result?.page.items ?? []).map((entry) => (
            <tr key={entry.id} className={entry.isOvertime ? "overtime" : undefined}>
              <td>{formatDate(entry.date)}</td>
              <td>{entry.employeeName}</td>
              <td title={entry.projectName}>{entry.projectCode}</td>
              <td className="num">
                {formatHours(entry.hours)}
                {entry.isOvertime && (
                  <span className="badge" title={`За день ${formatHours(entry.dayTotalHours)} ч`}>
                    переработка
                  </span>
                )}
              </td>
              <td className="num">{entry.rate === null ? "—" : formatMoney(entry.rate)}</td>
              <td className="num">{formatMoney(entry.amount)}</td>
              <td>{entry.comment}</td>
              <td>
                <button type="button" className="link" onClick={() => openEdit(entry)}>
                  Изменить
                </button>
              </td>
              <td>
                <button type="button" className="link danger" onClick={() => onDelete(entry)}>
                  Удалить
                </button>
              </td>
            </tr>
          ))}
          {result && result.page.items.length === 0 && !entries.isLoading && (
            <tr>
              <td colSpan={9} className="muted">
                За выбранный период записей нет.
              </td>
            </tr>
          )}
        </tbody>
        {result && (
          <tfoot>
            <tr>
              <td colSpan={3}>Итого по фильтру</td>
              <td className="num">{formatHours(result.totalHours)}</td>
              <td></td>
              <td className="num">{formatMoney(result.totalAmount)}</td>
              <td colSpan={3}></td>
            </tr>
          </tfoot>
        )}
      </table>

      {result && result.page.totalPages > 1 && (
        <div className="pagination">
          <button type="button" disabled={page <= 1} onClick={() => setPage(page - 1)}>
            ←
          </button>
          <span>
            Страница {result.page.page} из {result.page.totalPages} · всего записей {result.page.totalCount}
          </span>
          <button
            type="button"
            disabled={page >= result.page.totalPages}
            onClick={() => setPage(page + 1)}
          >
            →
          </button>
        </div>
      )}

      {dialogOpen && (
        <TimeEntryDialog
          entry={dialogEntry}
          employees={employees.data ?? []}
          projects={projects.data ?? []}
          defaultDate={`${monthValue(year, month)}-01`}
          onClose={() => setDialogOpen(false)}
        />
      )}
    </section>
  );
}
