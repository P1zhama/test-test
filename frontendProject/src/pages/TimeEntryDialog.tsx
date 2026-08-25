import { useState } from "react";
import { ApiError } from "../api/client";
import { useCreateTimeEntry, useUpdateTimeEntry } from "../api/hooks";
import type { Employee, Project, TimeEntry } from "../api/types";
import { Modal } from "../components/Modal";
import { toInputDate } from "../format";

interface Props {
  entry: TimeEntry | null;
  employees: Employee[];
  projects: Project[];
  defaultDate: string;
  onClose: () => void;
}

const HOURS_STEP = 0.5;
const MAX_HOURS = 24;

export function TimeEntryDialog({ entry, employees, projects, defaultDate, onClose }: Props) {
  const [employeeId, setEmployeeId] = useState(entry?.employeeId ?? "");
  const [projectId, setProjectId] = useState(entry?.projectId ?? "");
  const [date, setDate] = useState(entry ? toInputDate(entry.date) : defaultDate);
  const [hours, setHours] = useState(entry ? String(entry.hours) : "");
  const [comment, setComment] = useState(entry?.comment ?? "");
  const [localErrors, setLocalErrors] = useState<Record<string, string>>({});

  const create = useCreateTimeEntry();
  const update = useUpdateTimeEntry();
  const pending = create.isPending || update.isPending;
  const serverError = (update.error ?? create.error) as ApiError | null;

  const validate = (): Record<string, string> => {
    const errors: Record<string, string> = {};
    if (!employeeId) errors.employeeId = "Выберите сотрудника.";
    if (!projectId) errors.projectId = "Выберите проект.";
    if (!date) errors.date = "Укажите дату.";

    const parsed = Number(hours.replace(",", "."));
    if (!hours.trim() || Number.isNaN(parsed)) {
      errors.hours = "Укажите количество часов.";
    } else if (parsed <= 0) {
      errors.hours = "Часы должны быть больше 0.";
    } else if (parsed > MAX_HOURS) {
      errors.hours = `За одну запись нельзя указать больше ${MAX_HOURS} ч.`;
    } else if (Math.abs(parsed / HOURS_STEP - Math.round(parsed / HOURS_STEP)) > 1e-9) {
      errors.hours = "Часы должны быть кратны 0,5 (например 0,5; 1; 7,5).";
    }

    return errors;
  };

  const errorFor = (field: string): string | undefined =>
    localErrors[field] ?? serverError?.fieldError(field);

  const submit = async (event: React.FormEvent) => {
    event.preventDefault();

    const errors = validate();
    setLocalErrors(errors);
    if (Object.keys(errors).length > 0) return;

    const payload = {
      employeeId,
      projectId,
      date,
      hours: Number(hours.replace(",", ".")),
      comment: comment.trim() ? comment.trim() : null
    };

    try {
      if (entry) {
        await update.mutateAsync({ id: entry.id, payload: { ...payload, version: entry.version } });
      } else {
        await create.mutateAsync(payload);
      }
      onClose();
    } catch {
    }
  };

  return (
    <Modal title={entry ? "Изменение записи" : "Новая запись"} onClose={onClose}>
      <form onSubmit={submit} className="form">
        {serverError && serverError.errors.length === 0 && (
          <div className="banner error">
            <strong>{serverError.message}</strong>
            <span className="code">{serverError.code}</span>
          </div>
        )}

        <label>
          Сотрудник
          <select value={employeeId} onChange={(e) => setEmployeeId(e.target.value)}>
            <option value="">— выберите —</option>
            {employees.map((employee) => (
              <option key={employee.id} value={employee.id}>
                {employee.fullName}
              </option>
            ))}
          </select>
          {errorFor("employeeId") && <span className="field-error">{errorFor("employeeId")}</span>}
        </label>

        <label>
          Проект
          <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
            <option value="">— выберите —</option>
            {projects.map((project) => (
              <option key={project.id} value={project.id}>
                {project.code} — {project.name}
              </option>
            ))}
          </select>
          {errorFor("projectId") && <span className="field-error">{errorFor("projectId")}</span>}
        </label>

        <label>
          Дата
          <input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
          {errorFor("date") && <span className="field-error">{errorFor("date")}</span>}
        </label>

        <label>
          Часы
          {}
          <input
            type="number"
            step="0.1"
            min="0"
            value={hours}
            onChange={(e) => setHours(e.target.value)}
          />
          {errorFor("hours") && <span className="field-error">{errorFor("hours")}</span>}
        </label>

        <label>
          Комментарий
          <input type="text" value={comment} onChange={(e) => setComment(e.target.value)} />
          {errorFor("comment") && <span className="field-error">{errorFor("comment")}</span>}
        </label>

        <div className="form-actions">
          <button type="button" className="secondary" onClick={onClose}>
            Отмена
          </button>
          <button type="submit" disabled={pending}>
            {pending ? "Сохранение…" : "Сохранить"}
          </button>
        </div>
      </form>
    </Modal>
  );
}
