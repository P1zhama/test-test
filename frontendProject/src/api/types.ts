export interface Rate {
  from: string;
  value: number;
}

export interface Employee {
  id: string;
  fullName: string;
  department: string;
  rates: Rate[];
}

export interface Project {
  id: string;
  code: string;
  name: string;
  budget: number;
  startDate: string;
  endDate: string | null;
}

export interface TimeEntry {
  id: string;
  employeeId: string;
  employeeName: string;
  projectId: string;
  projectCode: string;
  projectName: string;
  date: string;
  hours: number;
  rate: number | null;
  amount: number;
  comment: string | null;
  isOvertime: boolean;
  dayTotalHours: number;
  version: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface TimeEntriesPage {
  page: PagedResult<TimeEntry>;
  totalHours: number;
  totalAmount: number;
}

export interface ProjectReportRow {
  projectId: string;
  projectCode: string;
  projectName: string;
  hours: number;
  amount: number;
  budget: number;
  percent: number | null;
  isOverspent: boolean;
  isAtRisk: boolean;
}

export interface ProjectReport {
  year: number;
  month: number;
  rows: ProjectReportRow[];
  totalHours: number;
  totalAmount: number;
}

export interface ClosedPeriod {
  year: number;
  month: number;
  closedAt: string;
}

export interface TimeEntryPayload {
  employeeId: string;
  projectId: string;
  date: string;
  hours: number;
  comment: string | null;
}
