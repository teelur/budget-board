import { useMediaQuery } from "@mantine/hooks";

const useIsMobile = () => useMediaQuery("(max-width: 48em)");

export default useIsMobile;
