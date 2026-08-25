const money = new Intl.NumberFormat("ru-RU", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const hours = new Intl.NumberFormat("ru-RU", { maximumFractionDigits: 2 });

export const formatMoney = (value: number): string => `${money.format(value)} ₽`;

export const formatHours = (value: number): string => hours.format(value);

export const formatPercent = (value: number | null): string =>
  value === null ? "—" : `${hours.format(value)} %`;

export const toInputDate = (isoDate: string): string => isoDate.slice(0, 10);

export const formatDate = (isoDate: string): string => {
  const [year, month, day] = toInputDate(isoDate).split("-");
  return `${day}.${month}.${year}`;
};

export const monthValue = (year: number, month: number): string =>
  `${year}-${String(month).padStart(2, "0")}`;

export const parseMonthValue = (value: string): { year: number; month: number } => {
  const [year, month] = value.split("-");
  return { year: Number(year), month: Number(month) };
};
