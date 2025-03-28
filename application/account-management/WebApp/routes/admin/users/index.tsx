import { SharedSideMenu } from "@/shared/components/SharedSideMenu";
import { TopMenu } from "@/shared/components/topMenu";
import { SortOrder, SortableUserProperties, UserRole, UserStatus, type components } from "@/shared/lib/api/client";
import { t } from "@lingui/core/macro";
import { Trans } from "@lingui/react/macro";
import { Breadcrumb } from "@repo/ui/components/Breadcrumbs";
import { Button } from "@repo/ui/components/Button";
import { createFileRoute } from "@tanstack/react-router";
import { PlusIcon } from "lucide-react";
import { useCallback, useState } from "react";
import { z } from "zod";
import InviteUserModal from "./-components/InviteUserModal";
import { UserTable } from "./-components/UserTable";
import { UserToolbar } from "./-components/UserToolbar";

type UserDetails = components["schemas"]["UserDetails"];

const userPageSearchSchema = z.object({
  search: z.string().optional(),
  userRole: z.nativeEnum(UserRole).nullable().optional(),
  userStatus: z.nativeEnum(UserStatus).nullable().optional(),
  startDate: z.string().optional(),
  endDate: z.string().optional(),
  orderBy: z.nativeEnum(SortableUserProperties).default(SortableUserProperties.Name).optional(),
  sortOrder: z.nativeEnum(SortOrder).default(SortOrder.Ascending).optional(),
  pageOffset: z.number().default(0).optional()
});

export const Route = createFileRoute("/admin/users/")({
  component: UsersPage,
  validateSearch: userPageSearchSchema
});

export default function UsersPage() {
  const [isInviteModalOpen, setIsInviteModalOpen] = useState(false);
  const [selectedUsers, setSelectedUsers] = useState<UserDetails[]>([]);
  const [refreshKey, setRefreshKey] = useState(0);

  const handleRefresh = useCallback(() => {
    setRefreshKey((prev) => prev + 1);
  }, []);

  return (
    <div className="flex h-full w-full gap-4">
      <SharedSideMenu ariaLabel={t`Toggle collapsed menu`} />
      <div className="flex w-full flex-col gap-4 px-4 py-3">
        <TopMenu>
          <Breadcrumb href="/admin/users">
            <Trans>Users</Trans>
          </Breadcrumb>
          <Breadcrumb>
            <Trans>All users</Trans>
          </Breadcrumb>
        </TopMenu>
        <div className="20 mb-4 flex w-full items-center justify-between space-x-2 sm:mt-4">
          <div className="mt-3 flex flex-col gap-2 font-semibold text-3xl text-foreground">
            <h1>
              <Trans>Users</Trans>
            </h1>
            <p className="font-normal text-muted-foreground text-sm">
              <Trans>Manage your users and permissions here.</Trans>
            </p>
          </div>
          <Button variant="primary" onPress={() => setIsInviteModalOpen(true)}>
            <PlusIcon className="h-4 w-4" />
            <Trans>Invite users</Trans>
          </Button>
        </div>

        <UserToolbar
          selectedUsers={selectedUsers}
          onUsersDeleted={() => setSelectedUsers([])}
          onRefreshNeeded={handleRefresh}
        />
        <UserTable
          key={refreshKey}
          selectedUsers={selectedUsers}
          onSelectedUsersChange={setSelectedUsers}
          onRefreshNeeded={handleRefresh}
        />
      </div>
      <InviteUserModal isOpen={isInviteModalOpen} onOpenChange={setIsInviteModalOpen} />
    </div>
  );
}
