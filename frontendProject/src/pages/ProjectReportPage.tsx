import { useProjectReport } from "../api/hooks";
import { ErrorBanner } from "../components/ErrorBanner";
import { MonthPicker } from "../components/MonthPicker";
import { formatHours, formatMoney, formatPercent } from "../format";

interface Props {
  year: number;
  month: number;
  onMonthChange: (year: number, month: number) => void;
}

export function ProjectReportPage({ year, month, onMonthChange }: Props) {
  const report = useProjectReport(year, month);

  return (
    <section>
      <div className="toolbar">
        <MonthPicker year={year} month={month} onChange={onMonthChange} />
      </div>

      <ErrorBanner error={report.error} />
      {report.isLoading && <div className="muted">Загрузка…</div>}

      <table>
        <thead>
          <tr>
            <th>Проект</th>
            <th className="num">Часы</th>
            <th className="num">Стоимость</th>
            <th className="num">Бюджет</th>
            <th className="num">Освоено</th>
          </tr>
        </thead>
        <tbody>
          {(report.data?.rows ?? []).map((row) => (
            <tr
              key={row.projectId}
              className={row.isOverspent ? "overspent" : row.isAtRisk ? "at-risk" : undefined}
            >
              <td>
                {row.projectCode} — {row.projectName}
              </td>
              <td className="num">{formatHours(row.hours)}</td>
              <td className="num">{formatMoney(row.amount)}</td>
              <td className="num">{formatMoney(row.budget)}</td>
              <td className="num">
                {formatPercent(row.percent)}
                {row.isOverspent && <span className="badge danger">перерасход</span>}
                {!row.isOverspent && row.isAtRisk && <span className="badge">риск</span>}
              </td>
            </tr>
          ))}
          {report.data && report.data.rows.length === 0 && (
            <tr>
              <td colSpan={5} className="muted">
                За выбранный месяц трудозатрат не было.
              </td>
            </tr>
          )}
        </tbody>
        {report.data && report.data.rows.length > 0 && (
          <tfoot>
            <tr>
              <td>Итого</td>
              <td className="num">{formatHours(report.data.totalHours)}</td>
              <td className="num">{formatMoney(report.data.totalAmount)}</td>
              <td colSpan={2}></td>
            </tr>
          </tfoot>
        )}
      </table>
    </section>
  );
}
