import { ApiError } from "../api/client";

interface Props {
  error: unknown;
}

export function ErrorBanner({ error }: Props) {
  if (!error) return null;

  if (error instanceof ApiError) {
    return (
      <div className="banner error">
        <strong>{error.message}</strong>
        {error.errors.length > 0 && (
          <ul>
            {error.errors.map((item, index) => (
              <li key={index}>
                {item.field}: {item.message}
              </li>
            ))}
          </ul>
        )}
        <span className="code">{error.code}</span>
      </div>
    );
  }

  return <div className="banner error">Непредвиденная ошибка. Подробности в консоли.</div>;
}
