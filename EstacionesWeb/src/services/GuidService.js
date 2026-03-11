// ===============================================================================================================
// GUID Service - JavaScript implementation for generating GUIDs
// ===============================================================================================================

class GuidService {
  constructor() {}

  generateGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
      const r = (Math.random() * 16) | 0
      const v = c === 'x' ? r : (r & 0x3) | 0x8
      return v.toString(16)
    })
  }

  // Additional utility methods for GUID validation
  isValidGuid(guid) {
    const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i
    return guidPattern.test(guid)
  }

  emptyGuid() {
    return '00000000-0000-0000-0000-000000000000'
  }
}

export default GuidService
