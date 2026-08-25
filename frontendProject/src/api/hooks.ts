import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "./client";
import type {
  ClosedPeriod,
  Employee,
  Project,
  ProjectReport,
  TimeEntriesPage,
  TimeEntryPayload
} from "./types";

export interface TimeEntriesFilter {
  year: number;
  month: number;
  employeeId: string;
  projectId: string;
  page: number;
  pageSize: number;
}

function timeEntriesUrl(filter: TimeEntriesFilter): string {
  const params = new URLSearchParams({
    year: String(filter.year),
    month: String(filter.month),
    page: String(filter.page),
    pageSize: String(filter.pageSize)
  });
  if (filter.employeeId) params.set("employeeId", filter.employeeId);
  if (filter.projectId) params.set("projectId", filter.projectId);
  return `/api/time-entries?${params.toString()}`;
}

export const useEmployees = () =>
  useQuery({
    queryKey: ["employees"],
    queryFn: () => api.get<Employee[]>("/api/employees")
  });

export const useProjects = () =>
  useQuery({
    queryKey: ["projects"],
    queryFn: () => api.get<Project[]>("/api/projects")
  });

export const useClosedPeriods = () =>
  useQuery({
    queryKey: ["periods"],
    queryFn: () => api.get<ClosedPeriod[]>("/api/periods")
  });

export const useTimeEntries = (filter: TimeEntriesFilter) =>
  useQuery({
    queryKey: ["time-entries", filter],
    queryFn: () => api.get<TimeEntriesPage>(timeEntriesUrl(filter))
  });

export const useProjectReport = (year: number, month: number) =>
  useQuery({
    queryKey: ["project-report", year, month],
    queryFn: () => api.get<ProjectReport>(`/api/reports/projects?year=${year}&month=${month}`)
  });

function useInvalidateAll() {
  const queryClient = useQueryClient();
  return () => {
    void queryClient.invalidateQueries({ queryKey: ["time-entries"] });
    void queryClient.invalidateQueries({ queryKey: ["project-report"] });
    void queryClient.invalidateQueries({ queryKey: ["periods"] });
  };
}

export const useCreateTimeEntry = () => {
  const invalidate = useInvalidateAll();
  return useMutation({
    mutationFn: (payload: TimeEntryPayload) => api.put("/api/time-entries", payload),
    onSuccess: invalidate
  });
};

export const useUpdateTimeEntry = () => {
  const invalidate = useInvalidateAll();
  return useMutation({
    mutationFn: (input: { id: string; payload: TimeEntryPayload & { version: number } }) =>
      api.post(`/api/time-entries/${input.id}`, input.payload),
    onSuccess: invalidate
  });
};

export const useDeleteTimeEntry = () => {
  const invalidate = useInvalidateAll();
  return useMutation({
    mutationFn: (id: string) => api.delete(`/api/time-entries/${id}`),
    onSuccess: invalidate
  });
};

export const useTogglePeriod = () => {
  const invalidate = useInvalidateAll();
  return useMutation({
    mutationFn: (input: { year: number; month: number; close: boolean }) =>
      api.post(input.close ? "/api/periods/close" : "/api/periods/open", {
        year: input.year,
        month: input.month
      }),
    onSuccess: invalidate
  });
};
