const summary = [
  {
    label: "Tracked wallets",
    value: "1 active connection",
  },
  {
    label: "Snapshot cadence",
    value: "Manual capture",
  },
  {
    label: "Coverage",
    value: "EVM chains",
  },
];

export default function Wallet() {
  return (
    <div className="mx-auto max-w-7xl">
      <header className="max-w-3xl">
        <p className="text-xs uppercase tracking-[0.35em] text-cyan-300/80">
          Wallet
        </p>
        <h2 className="mt-3 text-3xl font-semibold text-white">
          Start tracking a single portfolio
        </h2>
        <p className="mt-3 text-sm leading-6 text-white/60">
          This first pass focuses on the wallet overview so the later history,
          statistics, and asset views can plug into the same stored snapshot
          model.
        </p>
      </header>

      <section className="mt-8 grid gap-6 lg:grid-cols-[minmax(0,360px)_minmax(0,1fr)]">
        <div className="rounded-[28px] border border-white/10 bg-white/[0.03] p-6">
          <h3 className="text-lg font-semibold text-white">Connection plan</h3>
          <p className="mt-2 text-sm leading-6 text-white/60">
            The wallet form will store one address and chain combination per
            account before the backend starts persisting snapshots.
          </p>

          <div className="mt-6 space-y-3 text-sm text-white/70">
            <div className="rounded-2xl border border-dashed border-white/10 px-4 py-4">
              Wallet validation and save flow
            </div>
            <div className="rounded-2xl border border-dashed border-white/10 px-4 py-4">
              Initial snapshot capture after connect
            </div>
            <div className="rounded-2xl border border-dashed border-white/10 px-4 py-4">
              Stored value ready for timeline views
            </div>
          </div>
        </div>

        <div className="grid gap-4 md:grid-cols-3">
          {summary.map((item) => (
            <div
              key={item.label}
              className="rounded-[28px] border border-white/10 bg-white/[0.03] p-6"
            >
              <p className="text-xs uppercase tracking-[0.3em] text-white/45">
                {item.label}
              </p>
              <p className="mt-4 text-lg font-medium text-white">{item.value}</p>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
