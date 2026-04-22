import { NavLink, Outlet } from "react-router-dom";

const navigationItems = [
  { href: "/wallet", label: "Wallet" },
];

export default function PortfolioLayout() {
  return (
    <div className="min-h-screen bg-slate-950 text-white">
      <div className="mx-auto max-w-7xl px-6 pb-12 pt-8">
        <header className="rounded-[32px] border border-white/10 bg-white/[0.03] px-6 py-6 shadow-[0_24px_60px_rgba(2,6,23,0.4)]">
          <div className="flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
            <div className="max-w-2xl">
              <p className="text-xs uppercase tracking-[0.35em] text-cyan-300/80">
                Crypto tracker
              </p>
              <h1 className="mt-3 text-3xl font-semibold text-white">
                Portfolio dashboard
              </h1>
              <p className="mt-3 text-sm leading-6 text-white/60">
                Start with the wallet workspace and layer historical views on top
                as the stored portfolio data expands.
              </p>
            </div>

            <nav className="flex flex-wrap gap-3">
              {navigationItems.map((item) => (
                <NavLink
                  key={item.href}
                  to={item.href}
                  className={({ isActive }) =>
                    [
                      "rounded-full border px-4 py-2 text-sm font-medium transition",
                      isActive
                        ? "border-cyan-300/60 bg-cyan-300/15 text-cyan-200"
                        : "border-white/10 bg-white/[0.02] text-white/70 hover:border-white/20 hover:text-white",
                    ].join(" ")
                  }
                >
                  {item.label}
                </NavLink>
              ))}
            </nav>
          </div>
        </header>

        <main className="pt-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
