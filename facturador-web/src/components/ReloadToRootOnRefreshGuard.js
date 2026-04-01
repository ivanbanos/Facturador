import { useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";

function isPageReload() {
  const navigationEntries =
    window.performance?.getEntriesByType?.("navigation");

  if (navigationEntries && navigationEntries.length > 0) {
    return navigationEntries[0].type === "reload";
  }

  // Fallback for older browsers.
  return window.performance?.navigation?.type === 1;
}

function ReloadToRootOnRefreshGuard() {
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    // Evaluate only on first render to avoid forcing '/' on in-app navigation.
    if (isPageReload() && location.pathname !== "/") {
      navigate("/", { replace: true });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return null;
}

export default ReloadToRootOnRefreshGuard;
