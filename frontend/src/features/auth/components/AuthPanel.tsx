import { FormEvent, useState } from "react";

type Props = {
  onAuthenticate: (mode: "login" | "register", email: string, password: string) => Promise<void>;
};

export function AuthPanel({ onAuthenticate }: Props) {
  const [email, setEmail] = useState("emil@demo.dev");
  const [password, setPassword] = useState("password123");
  const [mode, setMode] = useState<"login" | "register">("login");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      await onAuthenticate(mode, email, password);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Auth failed.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="panel auth-panel">
      <div className="panel-header-row">
        <h2>Auth Views</h2>
        <div className="pill-toggle">
          <button
            type="button"
            className={mode === "login" ? "active" : ""}
            onClick={() => setMode("login")}
          >
            Login
          </button>
          <button
            type="button"
            className={mode === "register" ? "active" : ""}
            onClick={() => setMode("register")}
          >
            Register
          </button>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="stack">
        <label>
          Email
          <input
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            type="email"
            required
          />
        </label>

        <label>
          Password
          <input
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            type="password"
            required
          />
        </label>

        <button type="submit" disabled={loading}>
          {loading ? "Working..." : mode === "login" ? "Sign In" : "Create Account"}
        </button>

        {error && <p className="error-text">{error}</p>}
      </form>
    </section>
  );
}
