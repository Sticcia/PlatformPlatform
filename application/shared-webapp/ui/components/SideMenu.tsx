import type React from "react";
import logoMarkUrl from "../images/logo-mark.svg";
import logoWrapUrl from "../images/logo-wrap.svg";
import { ChevronsLeftIcon, type LucideIcon } from "lucide-react";
import { createContext, useContext } from "react";
import { tv } from "tailwind-variants";
import { Button } from "./Button";
import { useRouter } from "@tanstack/react-router";
import { Dialog, DialogTrigger } from "./Dialog";
import { Modal } from "./Modal";
import { Tooltip, TooltipTrigger } from "./Tooltip";
import type { Href } from "@react-types/shared";
import { useLocalStorage } from "../hooks/useLocalStorage";

const collapsedContext = createContext(false);

const menuButtonStyles = tv({
  base: "flex text-base font-normal w-full justify-start transition-all duration-300",
  variants: {
    isCollapsed: {
      true: "gap-0 ease-out",
      false: "gap-4 ease-in"
    }
  }
});

const menuTextStyles = tv({
  base: "text-foreground transition-all duration-300 text-start",
  variants: {
    isCollapsed: {
      true: "w-0 opacity-0 text-xs ease-out",
      false: "w-fit opacity-100 text-base ease-in"
    }
  }
});

type MenuButtonProps = {
  icon: LucideIcon;
  label: string;
  isDisabled?: boolean;
} & (
  | {
      forceReload?: false;
      href: Href;
    }
  | {
      forceReload: true;
      href: string;
    }
);

export function MenuButton({
  icon: Icon,
  label,
  href: to,
  isDisabled = false,
  forceReload = false
}: Readonly<MenuButtonProps>) {
  const isCollapsed = useContext(collapsedContext);
  const { navigate } = useRouter();
  const onPress = () => {
    if (to == null) return;
    if (forceReload) {
      window.location.href = to;
    } else {
      navigate({ to });
    }
  };

  return (
    <TooltipTrigger delay={300}>
      <Button variant="link" className={menuButtonStyles({ isCollapsed })} onPress={onPress} isDisabled={isDisabled}>
        <Icon className="w-6 h-6 shrink-0 grow-0" />
        <div className={menuTextStyles({ isCollapsed })}>{label}</div>
      </Button>
      {isCollapsed && <Tooltip placement="right">{label}</Tooltip>}
    </TooltipTrigger>
  );
}

const sideMenuStyles = tv({
  base: "relative hidden sm:flex flex-col pr-2 py-4 transition-all duration-300 items-start shrink-0 grow-0",
  variants: {
    isCollapsed: {
      true: "w-[72px] gap-2 pl-2 ease-out",
      false: "w-72 gap-4 pl-8 ease-in"
    }
  }
});

const chevronStyles = tv({
  base: "w-4 h-4 transition-all duration-300",
  variants: {
    isCollapsed: {
      true: "transform rotate-180 ease-out",
      false: "transform rotate-0 ease-in"
    }
  }
});

const logoWrapStyles = tv({
  base: "self-start transition-all duration-300",
  variants: {
    isCollapsed: {
      true: "h-8 opacity-0 ease-out",
      false: "h-8 ease-in opacity-100"
    }
  }
});

const logoMarkStyles = tv({
  base: "self-start transition-all duration-300",
  variants: {
    isCollapsed: {
      true: "h-8 opacity-100 ease-in",
      false: "h-8 opacity-0 ease-out"
    }
  }
});

type SideMenuProps = {
  children: React.ReactNode;
  ariaLabel: string;
};

export function SideMenu({ children, ariaLabel }: Readonly<SideMenuProps>) {
  const [isCollapsed, setIsCollapsed] = useLocalStorage<boolean>(
    !window.matchMedia("(min-width: 1024px)").matches,
    "side-menu-collapsed"
  );

  const toggleCollapse = () => {
    setIsCollapsed((v: boolean) => !v);
  };

  return (
    <>
      <collapsedContext.Provider value={isCollapsed}>
        <div className={sideMenuStyles({ isCollapsed })}>
          <div className="h-20">
            <Button
              variant="ghost"
              size="sm"
              onPress={toggleCollapse}
              className="absolute top-3.5 right-0 hover:bg-transparent hover:text-muted-foreground border-r-2 border-border rounded-r-none"
              aria-label={ariaLabel}
            >
              <ChevronsLeftIcon className={chevronStyles({ isCollapsed })} />
            </Button>
            <div className="pr-8">
              <img src={logoWrapUrl} alt="Logo Wrap" className={logoWrapStyles({ isCollapsed })} />
            </div>
            <div className="flex pl-3 pt-4">
              <img src={logoMarkUrl} alt="Logo" className={logoMarkStyles({ isCollapsed })} />
            </div>
          </div>
          {children}
        </div>
      </collapsedContext.Provider>
      <collapsedContext.Provider value={false}>
        <div className="absolute right-2 bottom-2 sm:hidden z-50">
          <DialogTrigger>
            <Button aria-label="Help" variant="icon">
              <img src={logoMarkUrl} alt="Logo" className="w-8 h-8" />
            </Button>
            <Modal position="left" fullSize>
              <Dialog className="w-60">
                <div className="pb-8">
                  <img src={logoWrapUrl} alt="Logo Wrap" />
                </div>
                {children}
              </Dialog>
            </Modal>
          </DialogTrigger>
        </div>
      </collapsedContext.Provider>
    </>
  );
}

const sideMenuSeparatorStyles = tv({
  base: "text-muted-foreground border-b-0 font-semibold uppercase transition-all duration-300 leading-4",
  variants: {
    isCollapsed: {
      true: "h-0 w-6 text-muted-foreground/0 border-b-4 border-border/100 text-[0px] pt-0 self-center ease-out",
      false: "h-8 w-full border-border/0 text-xs pt-4 ease-in"
    }
  }
});

type SideMenuSeparatorProps = {
  children: React.ReactNode;
};

export function SideMenuSeparator({ children }: Readonly<SideMenuSeparatorProps>) {
  const isCollapsed = useContext(collapsedContext);
  return (
    <div className="pl-4">
      <div className={sideMenuSeparatorStyles({ isCollapsed })}>{children}</div>
    </div>
  );
}

export function SideMenuSpacer() {
  return <div className="grow" />;
}
