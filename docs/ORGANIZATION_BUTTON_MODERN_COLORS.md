# Organization Pages - Modern Button Colors with Gray Balance

## Overview
Updated all organization management pages to use modern button colors from `app.css` with balanced gray tones for a sophisticated, professional look.

## Design Philosophy

### Color Balance Strategy
- **Primary Actions**: Use `--org-primary` (#2153C8) for important actions
- **Secondary Actions**: Use gray tones (`--neutral-gray-100`, `--neutral-gray-200`) for common actions
- **Outline Buttons**: Use gray borders with gray text for tertiary actions

### Button Classes

#### 1. `.btn-modern` (Base Class)
- Common styling for all modern buttons
- Features: inline-flex layout, padding, border-radius, transitions
- Hover effect: `translateY(-2px)` with enhanced shadow
- Active effect: Returns to normal position

```css
.btn-modern {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.75rem 1.5rem;
    border-radius: 12px;
    font-weight: 600;
    font-size: 0.95rem;
    border: none;
    cursor: pointer;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}
```

#### 2. `.btn-modern-primary`
- **Purpose**: Main call-to-action buttons
- **Background**: `var(--org-primary)` (#2153C8)
- **Text**: White
- **Hover**: Darker blue with shadow
- **Examples**: "리포트 출력", "저장", "확인"

```css
.btn-modern-primary {
    background: var(--org-primary);
    color: white;
    border: 2px solid transparent;
}

.btn-modern-primary:hover {
    background: var(--org-primary-hover);
    box-shadow: 0 4px 12px rgba(33, 83, 200, 0.25);
}
```

#### 3. `.btn-modern-secondary`
- **Purpose**: Common actions (refresh, cancel, etc.)
- **Background**: `var(--neutral-gray-100)` (#E0E0E0)
- **Text**: `var(--neutral-gray-800)` (#2E2E2E)
- **Hover**: Darker gray (`--neutral-gray-200`)
- **Examples**: "새로고침", "취소", "닫기"

```css
.btn-modern-secondary {
    background: var(--neutral-gray-100);
    color: var(--neutral-gray-800);
    border: 2px solid transparent;
}

.btn-modern-secondary:hover {
    background: var(--neutral-gray-200);
    color: var(--neutral-black);
}
```

#### 4. `.btn-modern-outline`
- **Purpose**: Tertiary actions or less emphasis
- **Background**: White
- **Border**: `var(--neutral-gray-300)` (#B0B0B0)
- **Text**: `var(--neutral-gray-600)` (#4A4A4A)
- **Hover**: Light gray background with darker border

```css
.btn-modern-outline {
    background: white;
    color: var(--neutral-gray-600);
    border: 2px solid var(--neutral-gray-300);
}

.btn-modern-outline:hover {
    background: var(--neutral-gray-050);
    border-color: var(--neutral-gray-600);
    color: var(--neutral-black);
}
```

## Updated Pages

### 1. OrganizationStructurePage.razor
**Location**: `src/NexaCRM.UI/Pages/OrganizationStructurePage.razor`

**Changes**:
- Updated button system with primary, secondary, and outline variants
- Primary button for "트리 보기" / "리스트 보기" toggle
- Secondary button for general actions

**Button Usage**:
```razor
<button class="btn-modern btn-modern-primary" @onclick="@(() => viewMode = "tree")">
    <i class="bi bi-diagram-3-fill"></i>
    <span>트리 보기</span>
</button>
<button class="btn-modern btn-modern-outline" @onclick="@(() => viewMode = "list")">
    <i class="bi bi-list-ul"></i>
    <span>리스트 보기</span>
</button>
```

### 2. OrganizationStatusPage.razor
**Location**: `src/NexaCRM.UI/Pages/OrganizationStatusPage.razor`

**Changes**:
- Added complete button style system
- Updated header buttons to use new classes
- "새로고침" uses secondary style (gray)
- "리포트 출력" uses primary style (blue)

**Button Usage**:
```razor
<button class="btn-modern btn-modern-secondary" @onclick="RefreshData">
    <i class="bi bi-arrow-clockwise"></i>
    <span>새로고침</span>
</button>
<button class="btn-modern btn-modern-primary" @onclick="ExportReport">
    <i class="bi bi-file-earmark-pdf"></i>
    <span>리포트 출력</span>
</button>
```

### 3. SystemAdminSettingsPage.razor
**Location**: `src/NexaCRM.UI/Pages/SystemAdminSettingsPage.razor`

**Changes**:
- Added complete button style system
- Updated "새로고침" button to secondary style (gray)
- Consistent with other organization pages

**Button Usage**:
```razor
<button class="btn-modern btn-modern-secondary" @onclick="RefreshData">
    <i class="bi bi-arrow-clockwise"></i>
    <span>새로고침</span>
</button>
```

## CSS Variables Used

### From app.css Organization Colors
```css
/* Primary Colors */
--org-primary: #2153C8;
--org-primary-hover: #1a4bb5;
--org-primary-light: rgba(33, 83, 200, 0.1);

/* Neutral Gray Palette */
--neutral-gray-050: #F5F5F5;
--neutral-gray-100: #E0E0E0;
--neutral-gray-200: #D6D6D6;
--neutral-gray-300: #B0B0B0;
--neutral-gray-600: #4A4A4A;
--neutral-gray-800: #2E2E2E;
--neutral-black: #000000;
```

## Visual Hierarchy

### Button Priority Levels
1. **Primary (Blue)**: Most important action on the page
   - Uses vibrant blue color
   - Stands out visually
   - Limited to 1-2 buttons per section

2. **Secondary (Gray)**: Common, frequently used actions
   - Uses neutral gray tones
   - Blends with interface
   - Can have multiple per section

3. **Outline (Gray Border)**: Less emphasis, optional actions
   - Minimal visual weight
   - Good for alternative actions
   - Uses gray borders to reduce prominence

## Benefits

### 1. Visual Balance
- Blue primary buttons provide clear call-to-action
- Gray secondary buttons reduce visual noise
- Creates professional, sophisticated appearance

### 2. Consistency
- Same button system across all organization pages
- Uses centralized CSS variables
- Easy to maintain and update

### 3. Accessibility
- Clear visual hierarchy
- Good color contrast ratios
- Hover states provide clear feedback

### 4. Modern Design
- Follows 2025 design trends
- Micro-interactions (translate, shadow)
- Smooth cubic-bezier transitions

## Build Status
✅ **Build Successful**
- Warnings: 38 (pre-existing)
- Errors: 0
- All button styles compile correctly
- Theme support maintained

## Future Enhancements
- Consider adding `.btn-modern-danger` for destructive actions
- Add `.btn-modern-success` for positive confirmations
- Implement loading states for async actions
- Add disabled states with reduced opacity
