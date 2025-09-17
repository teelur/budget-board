import classes from "./AccountsSettings.module.css";

import { ActionIcon, useModalsStack } from "@mantine/core";
import { SettingsIcon } from "lucide-react";
import React from "react";
import AccountsSettingsModal from "./AccountsSettingsModal/AccountsSettingsModal";
import { IInstitution } from "~/models/institution";
import { IAccountResponse } from "~/models/account";
import CreateAccountModal from "./CreateAccountModal/CreateAccountModal";

interface AccountsSettingsProps {
  sortedFilteredInstitutions: IInstitution[];
  accounts: IAccountResponse[];
}

const AccountsSettings = (props: AccountsSettingsProps): React.ReactNode => {
  const stack = useModalsStack(["settings", "createAccount"]);

  return (
    <div>
      <AccountsSettingsModal
        sortedFilteredInstitutions={props.sortedFilteredInstitutions}
        accounts={props.accounts}
        onCreateAccountClick={() => stack.open("createAccount")}
        {...stack.register("settings")}
      />
      <CreateAccountModal {...stack.register("createAccount")} />
      <ActionIcon
        className={classes.settingsIcon}
        variant="subtle"
        onClick={() => stack.open("settings")}
      >
        <SettingsIcon />
      </ActionIcon>
    </div>
  );
};
export default AccountsSettings;
