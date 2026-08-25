import { monthValue, parseMonthValue } from "../format";

interface Props {
  year: number;
  month: number;
  onChange: (year: number, month: number) => void;
}

export function MonthPicker({ year, month, onChange }: Props) {
  return (
    <label>
      Месяц
      <input
        type="month"
        value={monthValue(year, month)}
        onChange={(event) => {
          if (!event.target.value) return;
          const parsed = parseMonthValue(event.target.value);
          onChange(parsed.year, parsed.month);
        }}
      />
    </label>
  );
}
