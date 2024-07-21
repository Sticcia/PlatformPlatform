import { DotIcon } from "lucide-react";
import { Trans } from "@lingui/macro";
import { useLingui } from "@lingui/react";
import { Navigate } from "@tanstack/react-router";
import { useActionState } from "react";
import type { State } from "./actions";
import { startAccountRegistration } from "./actions";
import { Button } from "@repo/ui/components/Button";
import { Description } from "@repo/ui/components/Description";
import { FieldError } from "@repo/ui/components/FieldError";
import { Form } from "@repo/ui/components/Form";
import { Heading } from "@repo/ui/components/Heading";
import { Input } from "@repo/ui/components/Input";
import { Label } from "@repo/ui/components/Label";
import { Link } from "@repo/ui/components/Link";
import { Select, SelectItem } from "@repo/ui/components/Select";
import { DomainInput } from "@repo/ui/components/DomainInput";
import logoMarkUrl from "@/shared/images/logo-mark.svg";
import poweredByUrl from "@/shared/images/powered-by.svg";
import { TextField } from "@repo/ui/components/TextField";

export function StartAccountRegistrationForm() {
  const { i18n } = useLingui();
  const initialState: State = { message: null, errors: {} };

  const [state, action] = useActionState(startAccountRegistration, initialState);

  if (state.success) {
    return <Navigate to="/register/verify" />;
  }
  return (
    <Form
      action={action}
      validationErrors={state.errors}
      className="flex w-full max-w-sm flex-col items-center gap-4 space-y-3 rounded-lg px-6 pt-8 pb-4"
    >
      <Link href="/">
        <img src={logoMarkUrl} className="h-12 w-12" alt="logo mark" />
      </Link>
      <Heading className="text-2xl">Create your account</Heading>
      <div className="text-center text-muted-foreground text-sm">
        Sign up in seconds to get started building on PlatformPlatform - just like thousands of others.
      </div>
      <TextField className="flex w-full flex-col">
        <Label>
          <Trans>Email</Trans>
        </Label>
        <Input
          type="email"
          name="email"
          autoFocus
          autoComplete="email webauthn"
          required
          placeholder={i18n.t("yourname@example.com")}
        />
        <FieldError />
      </TextField>
      <TextField className="flex w-full flex-col">
        <Label>
          <Trans>Subdomain</Trans>
        </Label>
        <DomainInput name="subdomain" domain=".platformplatform.net" required placeholder="subdomain" />
        <FieldError />
      </TextField>
      <TextField className="flex w-full flex-col">
        <Label>
          <Trans>Region</Trans>
        </Label>
        <Select name="region" selectedKey="europe" key="europe">
          <SelectItem id="europe">Europe</SelectItem>
        </Select>
        <Description>
          <Trans>This is the region where your data is stored</Trans>
        </Description>
        <FieldError />
      </TextField>
      <Button type="submit" className="mt-4 w-full text-center">
        Create your account
      </Button>
      <p className="text-muted-foreground text-xs">
        <Trans>Already have an account?</Trans>
        <Link href="/login/">
          <Trans>Log in</Trans>
        </Link>
      </p>
      <div className="text-muted-foreground text-sm">
        By continuing, you agree to our policies
        <div className="flex items-center justify-center">
          <Link href="/">Terms of use</Link>
          <DotIcon className="h-4 w-4" />
          <Link href="/">Privacy Policies</Link>
        </div>
      </div>
      <img src={poweredByUrl} alt="powered by" />
    </Form>
  );
}
