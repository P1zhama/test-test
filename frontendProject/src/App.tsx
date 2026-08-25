import { useState } from "react";
import { ProjectReportPage } from "./pages/ProjectReportPage";
import { TimeEntriesPage } from "./pages/TimeEntriesPage";

type Tab = "entries" | "report";

export function App() {
  const [tab, setTab] = useState<Tab>("entries");
  const [year, setYear] = useState(2026);
  const [month, setMonth] = useState(3);

  const onMonthChange = (nextYear: number, nextMonth: number) => {
    setYear(nextYear);
    setMonth(nextMonth);
  };

  return (
    <div className="app">
      <header>
        <h1>Учёт трудозатрат</h1>
        <nav>
          <button
            type="button"
            className={tab === "entries" ? "tab active" : "tab"}
            onClick={() => setTab("entries")}
          >
            Табель
          </button>
          <button
            type="button"
            className={tab === "report" ? "tab active" : "tab"}
            onClick={() => setTab("report")}
          >
            Отчёт по проектам
          </button>
        </nav>
      </header>

      {tab === "entries" ? (
        <TimeEntriesPage year={year} month={month} onMonthChange={onMonthChange} />
      ) : (
        <ProjectReportPage year={year} month={month} onMonthChange={onMonthChange} />
      )}
    </div>
  );
}
