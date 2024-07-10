import { useEffect, useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { TopMenu } from "@/pages/admin/users/-components/TopMenu";
import { SideMenu } from "@repo/ui/components/SideMenu";
import { Trans } from "@lingui/macro";
import { accountManagementApi } from "@/shared/lib/api/client";

export const Route = createFileRoute("/admin/")({
  component: Home
});

export default function Home() {
  console.log("Home component rendered");

  const [totalCount, setTotalCount] = useState<number | null>(null);

  useEffect(() => {
    accountManagementApi
      .GET("/api/account-management/users", {
        params: {
          query: {
            PageSize: 1
          }
        }
      })
      .then(({ data }) =>        setTotalCount(data?.totalCount ?? null))
      .catch((e) => console.error(e));
  }, []);

  return (
    <div className="flex gap-4 w-full h-full border">
      <SideMenu />
      <div className="flex flex-grow flex-col gap-4 pl-1 pr-6 py-3 overflow-x-auto">
        <div className="z-10">
          <TopMenu />
        </div>
        <div className="flex h-24 items-center justify-between space-x-2 mt-4 mb-4">
          <div className="text-foreground text-3xl font-semibold flex gap-2 flex-col mt-6">
            <h1 className="muted-foreground ">
              <Trans>Welcome home</Trans>
            </h1>
            <p className="text-muted-foreground text-sm font-normal whitespace-nowrap overflow-hidden text-ellipsis">
              <Trans>Here’s your overview of what’s going on.</Trans>
            </p>
          </div>
        </div>
        <div className="flex flex-row">
          <div className="text-muted-foreground p-6 bg-white rounded-xl shadow-md w-1/3">
            <div className="text-sm text-gray-800">
              <Trans>Total Users</Trans>
            </div>
            <div className="text-sm text-gray-500">
              <Trans>Add more in the Users menu</Trans>
            </div>
            <div className="py-2 text-black text-2xl font-semibold">{totalCount ? <p>{totalCount}</p> : <p>-</p>}</div>
          </div>
          <div className="p-6 bg-white rounded-xl shadow-md w-1/3 mx-6">
            <div className="text-sm text-gray-800">
              <Trans>Active Users</Trans>
            </div>
            <div className="text-sm text-gray-500">
              <Trans>Active users the past 30 days</Trans>
            </div>
            <div className="py-2 text-black text-2xl font-semibold">{totalCount ? <p>{totalCount}</p> : <p>-</p>}</div>
          </div>
        </div>
      </div>
    </div>
  );
}
