import { useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";

function isPageReload() {
  const navigationEntries = window.performance?.getEntriesByType?.("navigation");

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
    if (isPageReload() && location.pathname !== "/") {
      navigate("/", { replace: true });
    }
  }, [location.pathname, navigate]);

  return null;
}

export default ReloadToRootOnRefreshGuard;
