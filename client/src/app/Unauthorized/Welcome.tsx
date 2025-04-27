import React from "react";
import BudgetBoardLogo from "~/assets/budget-board-logo";
import {
  Container,
  Text,
  Stack,
  Group,
  Anchor,
  useComputedColorScheme,
  Card,
} from "@mantine/core";
import Register from "./Register";
import Login from "./Login";
import ResetPassword from "./ResetPassword";

export enum LoginCardState {
  Login,
  ResetPassword,
  Register,
}

const Welcome = (): React.ReactNode => {
  const [loginCardState, setLoginCardState] = React.useState<LoginCardState>(
    LoginCardState.Login
  );
  const [userEmail, setUserEmail] = React.useState<string>("");
  const computedColorScheme = useComputedColorScheme();

  const getCardState = (): React.ReactNode => {
    switch (loginCardState) {
      case LoginCardState.Login:
        return (
          <Login
            setLoginCardState={setLoginCardState}
            setUserEmail={setUserEmail}
          />
        );
      case LoginCardState.ResetPassword:
        return (
          <ResetPassword
            setLoginCardState={setLoginCardState}
            email={userEmail}
          />
        );
      case LoginCardState.Register:
        return <Register setLoginCardState={setLoginCardState} />;
      default:
        return <>There was an error.</>;
    }
  };

  return (
    <Container size="lg" w="500px" maw="100%">
      <Stack align="center" gap={10}>
        <Text size="xl">Welcome to</Text>
        <BudgetBoardLogo
          width={340}
          darkMode={computedColorScheme === "dark"}
        />
        <Text size="md">A simple app for managing monthly budgets.</Text>
      </Stack>
      <Card shadow="sm" withBorder mt="2em" w="100%" maw="500px" p={20}>
        {getCardState()}
      </Card>
      {loginCardState !== LoginCardState.Register && (
        <Group mt="xl" justify="center">
          <Text size="sm">Don't have an account?</Text>
          <Anchor
            size="sm"
            onClick={() => setLoginCardState(LoginCardState.Register)}
          >
            Register here
          </Anchor>
        </Group>
      )}
    </Container>
  );
};

export default Welcome;
