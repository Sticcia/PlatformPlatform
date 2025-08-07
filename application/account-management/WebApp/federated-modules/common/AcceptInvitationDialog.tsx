import type { components } from "@/shared/lib/api/api.generated";
import { api } from "@/shared/lib/api/client";
import { Trans } from "@lingui/react/macro";
import { Button } from "@repo/ui/components/Button";
import { Dialog } from "@repo/ui/components/Dialog";
import { DialogContent, DialogFooter, DialogHeader } from "@repo/ui/components/DialogFooter";
import { Heading } from "@repo/ui/components/Heading";
import { Modal } from "@repo/ui/components/Modal";
import { Text } from "@repo/ui/components/Text";
import { XIcon } from "lucide-react";

type TenantInfo = components["schemas"]["TenantInfo"];

interface AcceptInvitationDialogProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  tenant: TenantInfo | null;
  onAccept: () => void;
  isLoading?: boolean;
}

export function AcceptInvitationDialog({
  isOpen,
  onOpenChange,
  tenant,
  onAccept,
  isLoading = false
}: Readonly<AcceptInvitationDialogProps>) {
  const declineInvitationMutation = api.useMutation("post", "/api/account-management/users/decline-invitation", {
    onSuccess: () => {
      onOpenChange(false);
      window.location.reload();
    }
  });

  const handleDeclineInvitation = () => {
    if (tenant) {
      declineInvitationMutation.mutate({ body: { tenantId: tenant.tenantId } });
    }
  };

  if (!tenant) {
    return null;
  }

  return (
    <Modal isOpen={isOpen} onOpenChange={onOpenChange} isDismissable={true}>
      <Dialog className="sm:max-w-lg">
        <XIcon
          onClick={() => onOpenChange(false)}
          className="absolute top-2 right-2 h-10 w-10 cursor-pointer p-2 hover:bg-muted"
        />
        <DialogHeader>
          <Heading slot="title" className="text-2xl">
            <Trans>Accept invitation</Trans>
          </Heading>
        </DialogHeader>
        <DialogContent className="flex flex-col gap-4">
          <Text>
            <Trans>
              You have been invited to join <strong>{tenant.tenantName}</strong>.
            </Trans>
          </Text>
          <Text className="text-muted-foreground text-sm">
            <Trans>
              When you accept this invitation, your profile information (name, title, and avatar) from your current
              account will be copied to the new account.
            </Trans>
          </Text>
        </DialogContent>
        <DialogFooter>
          <Button
            variant="destructive"
            onPress={handleDeclineInvitation}
            isDisabled={isLoading || declineInvitationMutation.isPending}
          >
            {declineInvitationMutation.isPending ? <Trans>Declining...</Trans> : <Trans>Decline</Trans>}
          </Button>
          <Button variant="primary" onPress={onAccept} isDisabled={isLoading || declineInvitationMutation.isPending}>
            {isLoading ? <Trans>Accepting...</Trans> : <Trans>Accept invitation</Trans>}
          </Button>
        </DialogFooter>
      </Dialog>
    </Modal>
  );
}
