import { MantineColorScheme, useMantineColorScheme } from "@mantine/core";
import Select from "~/components/Select/Select/Select";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";

const DarkModeToggle = () => {
  const darkModeOptions = [
    { label: "Auto", value: "auto" },
    { label: "Light", value: "light" },
    { label: "Dark", value: "dark" },
  ];
  const { colorScheme, setColorScheme } = useMantineColorScheme();

  return (
    <Select
      data={darkModeOptions}
      label={<PrimaryText size="sm">Appearance Mode</PrimaryText>}
      value={colorScheme}
      onChange={(value) => setColorScheme(value as MantineColorScheme)}
      elevation={0}
    />
  );
};

export default DarkModeToggle;
