import { useCallback, useEffect } from "react";
import { Button } from "@repo/ui/components/Button";
import { TextField } from "@repo/ui/components/TextField";
import { Heading } from "@repo/ui/components/Heading";
import { Form } from "@repo/ui/components/Form";
import { XIcon } from "lucide-react";
import { Dialog } from "@repo/ui/components/Dialog";
import { Modal } from "@repo/ui/components/Modal";
import { api } from "@/shared/lib/api/client";
import { t } from "@lingui/core/macro";
import { Trans } from "@lingui/react/macro";
import { FormErrorMessage } from "@repo/ui/components/FormErrorMessage";
import { mutationSubmitter } from "@repo/ui/forms/mutationSubmitter";

type InviteUserModalProps = {
  isOpen: boolean;
  onOpenChange: (isOpen: boolean) => void;
};

export default function InviteUserModal({ isOpen, onOpenChange }: Readonly<InviteUserModalProps>) {
  const closeDialog = useCallback(() => {
    onOpenChange(false);
  }, [onOpenChange]);

  const inviteUserMutation = api.useMutation("post", "/api/account-management/users/invite");

  useEffect(() => {
    if (inviteUserMutation.isSuccess) {
      closeDialog();
      window.location.reload();
    }
  }, [inviteUserMutation.isSuccess, closeDialog]);

  return (
    <Modal isOpen={isOpen} onOpenChange={onOpenChange} isDismissable={true}>
      <Dialog>
        <XIcon onClick={closeDialog} className="h-10 w-10 absolute top-2 right-2 p-2 hover:bg-muted" />
        <Heading slot="title" className="text-2xl">
          <Trans>Invite User</Trans>
        </Heading>
        <p className="text-muted-foreground text-sm">
          <Trans>Invite users and assign them roles. They will appear once they log in.</Trans>
        </p>

        <Form
          onSubmit={mutationSubmitter(inviteUserMutation)}
          validationErrors={inviteUserMutation.error?.errors}
          validationBehavior="aria"
          className="flex flex-col gap-4 mt-4"
        >
          <TextField
            autoFocus
            isRequired
            name="email"
            label={t`Email`}
            placeholder={t`user@email.com`}
            className="flex-grow"
          />
          <FormErrorMessage error={inviteUserMutation.error} />
          <div className="flex justify-end gap-4 mt-6">
            <Button type="reset" onPress={closeDialog} variant="secondary">
              <Trans>Cancel</Trans>
            </Button>
            <Button type="submit" isDisabled={inviteUserMutation.isPending}>
              <Trans>Send invite</Trans>
            </Button>
          </div>
        </Form>
      </Dialog>
    </Modal>
  );
}
