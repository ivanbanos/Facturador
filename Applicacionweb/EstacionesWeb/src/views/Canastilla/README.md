# CanastillaItem Component

This component provides a comprehensive interface for managing basket items (CanastillaItem) with full CRUD operations.

## Features

### Data Management
- **List View**: Display all basket items in a responsive table
- **Add Items**: Create new basket items with validation
- **Edit Items**: Update existing basket items
- **Soft Delete**: Mark items as deleted without removing from database
- **Restore**: Restore soft-deleted items

### User Interface
- **Search**: Filter items by description, unit, or basket ID
- **Status Filter**: Toggle between active and deleted items
- **Real-time Updates**: Automatic refresh after operations
- **Responsive Design**: Works on desktop and mobile devices

### Data Fields

Each CanastillaItem contains:
- **CanastillaId** (int): ID of the parent basket
- **descripcion** (string): Item description
- **unidad** (string): Unit of measurement (Kg, Unit, Liter, etc.)
- **precio** (float): Base price
- **iva** (int): IVA percentage (0-100)
- **campoextra** (string): Additional text field for extra information (optional)
- **deleted** (bool): Soft delete flag
- **guid** (Guid): Unique identifier

### Calculated Fields
- **Total with IVA**: Automatically calculated as precio + (precio * iva/100)
- **Formatted Prices**: Display in Colombian Peso (COP) currency format

## Usage

### Navigation
Access via: `/CanastillaItem` or through the main navigation menu under "Items de Canastilla"

### Basic Operations

#### Adding Items
1. Click "Agregar Item" button
2. Fill required fields:
   - Canastilla ID (required)
   - Description (required)
   - Unit (required)
   - Price (must be > 0)
   - IVA percentage (0-100)
   - Campo Extra (optional additional information)
3. Preview total with IVA
4. Click "Guardar"

#### Editing Items
1. Click the edit icon (pencil) on any item row
2. Modify fields as needed
3. Click "Guardar"

#### Deleting Items
1. Click the delete icon (trash) on any item row
2. Confirm deletion
3. Item is marked as deleted (soft delete)

#### Restoring Items
1. Switch to "Items Eliminados" view
2. Click the restore icon (reload) on deleted items
3. Item is restored to active status

### Search and Filtering
- Use the search box to filter by description, unit, basket ID, or campo extra
- Toggle between "Items Activos" and "Items Eliminados"
- Click "Actualizar" to refresh the data

## Technical Details

### Services Used
- **CanastillaItemService**: Main service for CRUD operations
- **GuidService**: For generating unique identifiers
- **Toast**: For user notifications

### API Endpoints
The component expects these endpoints to be available:
- `GET /CanastillaItem` - Get all items
- `GET /CanastillaItem/{id}` - Get specific item
- `GET /CanastillaItem/canastilla/{canastillaId}` - Get items by basket ID
- `POST /CanastillaItem` - Create/update item
- `PUT /CanastillaItem/{id}` - Update specific item
- `DELETE /CanastillaItem/{id}` - Hard delete item
- `PATCH /CanastillaItem/{id}/soft-delete` - Soft delete item
- `PATCH /CanastillaItem/{id}/restore` - Restore deleted item

### Validation Rules
- Canastilla ID: Required, must be a valid integer
- Description: Required, cannot be empty
- Unit: Required, cannot be empty
- Price: Required, must be greater than 0
- IVA: Optional, must be between 0 and 100

### Currency Formatting
Prices are formatted using Colombian Peso (COP) locale:
```javascript
new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
}).format(price)
```

## Dependencies

### React Packages
- React hooks (useState, useEffect, useRef)
- React Router (useNavigate)

### CoreUI Components
- CCard, CCardBody, CCardHeader
- CTable, CTableBody, CTableHead, etc.
- CButton, CButtonGroup
- CModal, CModalHeader, CModalBody, CModalFooter
- CFormInput, CFormSelect
- CAlert, CBadge, CSpinner
- CIcon

### Icons
- cilPencil, cilTrash, cilPlus, cilSave, cilX, cilReload, cilSearch

## Error Handling
- Form validation with real-time feedback
- API error handling with user-friendly messages
- Loading states during operations
- Confirmation dialogs for destructive actions

## Responsive Design
- Table is responsive and scrollable on small screens
- Modal forms adapt to screen size
- Button groups stack appropriately on mobile devices
