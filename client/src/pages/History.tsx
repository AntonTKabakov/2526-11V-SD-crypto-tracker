import { useEffect, useState } from "react";

import { getApiErrorMessage } from "@/api/client";
import {
  getSnapshotHistory,
  getWalletConnection,
  type WalletConnection,
  type WalletSnapshot,
} from "@/api/portfolio";
import DashboardCard from "@/components/dashboard-card";
import DashboardPageHeader from "@/components/dashboard-page-header";
import { formatAmount, formatCurrency, formatDateTime } from "@/lib/formatters";

export default function History() {
  const [walletConnection, setWalletConnection] = useState<WalletConnection | null>(null);
  const [snapshots, setSnapshots] = useState<WalletSnapshot[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    let isMounted = true;

    const run = async () => {
      try {
        const [walletResult, snapshotsResult] = await Promise.all([
          getWalletConnection(),
          getSnapshotHistory(),
        ]);

        if (!isMounted) {
          return;
        }

        setWalletConnection(walletResult);
        setSnapshots(snapshotsResult);
      } catch (error) {
        if (!isMounted) {
          return;
        }

        setErrorMessage(getApiErrorMessage(error));
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    run();

    return () => {
      isMounted = false;
    };
  }, []);

  return (
    <div className="mx-auto max-w-7xl px-6">
      <DashboardPageHeader
        eyebrow="History"
        title="Immutable portfolio snapshots"
        description="Each timeline entry represents a full stored wallet state at a point in time. Historical graphing and breakdowns come from these database snapshots only."
      />

      {isLoading ? (
        <DashboardCard>
          <p className="text-white/70">Loading snapshot history...</p>
        </DashboardCard>
      ) : errorMessage ? (
        <DashboardCard>
          <p className="text-sm text-rose-300">{errorMessage}</p>
        </DashboardCard>
      ) : !walletConnection?.isConnected ? (
        <DashboardCard>
          <p className="text-white/70">
            Connect a wallet to start building a snapshot timeline.
          </p>
        </DashboardCard>
      ) : snapshots.length === 0 ? (
        <DashboardCard>
          <p className="text-white/70">
            No snapshots exist yet for the connected wallet.
          </p>
        </DashboardCard>
      ) : (
        <div className="space-y-6">
          {snapshots.map((snapshot) => (
            <DashboardCard className="overflow-hidden p-0" key={snapshot.id}>
              <div className="flex flex-wrap items-center justify-between gap-4 border-b border-white/10 px-6 py-5">
                <div>
                  <h2 className="text-lg font-semibold text-white">
                    {formatDateTime(snapshot.timestamp)}
                  </h2>
                  <p className="mt-1 text-sm text-white/60">
                    {snapshot.walletAddress} on {snapshot.chain.toUpperCase()}
                  </p>
                </div>
                <div className="text-right">
                  <p className="text-xs uppercase tracking-[0.3em] text-white/45">
                    Portfolio value
                  </p>
                  <p className="mt-2 text-lg font-semibold text-[#00F5C8]">
                    {formatCurrency(snapshot.totalValueUsd)}
                  </p>
                </div>
              </div>

              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-white/10">
                  <thead className="bg-white/[0.03]">
                    <tr className="text-left text-xs uppercase tracking-[0.3em] text-white/45">
                      <th className="px-6 py-4">Asset</th>
                      <th className="px-6 py-4 text-right">Balance</th>
                      <th className="px-6 py-4 text-right">Price</th>
                      <th className="px-6 py-4 text-right">Value</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-white/10">
                    {snapshot.assets.map((asset) => (
                      <tr
                        className="transition-colors duration-200 hover:bg-white/[0.03]"
                        key={asset.assetId}
                      >
                        <td className="px-6 py-5">
                          <div className="font-medium text-white">{asset.assetName}</div>
                          <div className="mt-1 text-xs uppercase tracking-[0.25em] text-[#00F5C8]">
                            {asset.assetSymbol}
                          </div>
                        </td>
                        <td className="px-6 py-5 text-right text-sm text-white/80">
                          {formatAmount(asset.amountHeld)}
                        </td>
                        <td className="px-6 py-5 text-right text-sm text-white/80">
                          {formatCurrency(asset.priceUsd)}
                        </td>
                        <td className="px-6 py-5 text-right text-sm text-white">
                          {formatCurrency(asset.currentValue)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </DashboardCard>
          ))}
        </div>
      )}
    </div>
  );
}
