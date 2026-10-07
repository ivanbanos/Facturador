// ===============================================================================================================
// FeatureFlags - funcionalidades opcionales configuradas en public/config.js (window.FEATURE_FLAGS)
// ===============================================================================================================

export const FEATURE_USUARIOS_CLIENTES = 'usuarios-clientes'

// Una funcionalidad esta activa solo si su clave vale true en config.js
export const isFeatureEnabled = (nombre) => window.FEATURE_FLAGS?.[nombre] === true
