import { FormEvent, useState } from "react";

type Props = {
  onSearch: (query: string) => Promise<void>;
  onFilter: (assignee: string, label: string) => Promise<void>;
};

export function SearchPanel({ onSearch, onFilter }: Props) {
  const [query, setQuery] = useState("");
  const [assignee, setAssignee] = useState("");
  const [label, setLabel] = useState("");

  const submitSearch = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    await onSearch(query);
  };

  const submitFilter = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    await onFilter(assignee, label);
  };

  return (
    <section className="panel">
      <h2>Live Search and Filtering</h2>
      <div className="search-grid">
        <form className="stack" onSubmit={submitSearch}>
          <label>
            Search tasks
            <input
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder="Try: auth, signalr, backend"
            />
          </label>
          <button type="submit">Run Search</button>
        </form>

        <form className="stack" onSubmit={submitFilter}>
          <label>
            Assignee
            <input
              value={assignee}
              onChange={(event) => setAssignee(event.target.value)}
              placeholder="Emil"
            />
          </label>
          <label>
            Label
            <input
              value={label}
              onChange={(event) => setLabel(event.target.value)}
              placeholder="frontend"
            />
          </label>
          <button type="submit">Apply Filter</button>
        </form>
      </div>
    </section>
  );
}
