// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

import { defineComponent, PropType, shallowRef, ShallowRef, unref, VNode, watch, WatchStopHandle } from "vue";
import { NumberFilterMethod } from "@Obsidian/Enums/Controls/Grid/numberFilterMethod";
import { GridColumnFilter, GridColumnDefinition, IGridState, StandardFilterProps, StandardCellProps, IGridCache, IGridRowCache, ValueFormatterFunction, ColumnSort, SortValueFunction, FilterValueFunction, QuickFilterValueFunction, UniqueValueFunction } from "@Obsidian/Types/Controls/grid";
import { getVNodeProp, getVNodeProps } from "@Obsidian/Utility/component";
import { resolveMergeFields } from "@Obsidian/Utility/lava";
import { deepEqual } from "@Obsidian/Utility/util";
import { AttributeFieldDefinitionBag } from "@Obsidian/ViewModels/Core/Grid/attributeFieldDefinitionBag";

// #region Standard Component Props

export const standardColumnProps = {
    /** The name that uniquely identifies this column in the grid. */
    name: {
        type: String as PropType<string>,
        default: ""
    },

    /** The title of the column, this is displayed in the table header. */
    title: {
        type: String as PropType<string>,
        required: false
    },

    /**
     * The name of the field on the row that will contain the data. This is
     * used by default columns and other features to automatically display
     * the data. If you are building a completely custom column it is not
     * required.
     */
    field: {
        type: String as PropType<string>,
        required: false
    },

    /**
     * Overrides the default method of obtaining the value to use when matching
     * against the quick filter. If not specified then the value of of the row
     * in the `field` property will be used if it is a supported type. A
     * function may be specified which will be called with the row and column
     * definition and must return either a string or undefined. If a plain
     * string is specified then it will be used as a Lava Template which will
     * be passed the `row` object.
     */
    quickFilterValue: {
        type: Object as PropType<QuickFilterValueFunction | string>,
        required: false
    },

    /**
     * The name of the field on the row that will contain the data to be used
     * when sorting. If this is not specified then the value from `field` will
     * be used by default. If no `title` is specified then the column will not
     * be sortable.
     */
    sortField: {
        type: String as PropType<string>,
        required: false
    },

    /**
     * Specifies how to get the sort value to use when sorting by this column.
     * This will override the `sortField` setting. If a function is be provided
     * then it will be called with the row and the column definition and must
     * return either a string, number or undefined. If a string is provided
     * then it will be used as a Lava Template which will be passed the `row`
     * object used to calculate the value. If no `title` is specified then the
     * column will not be sortable.
     */
    sortValue: {
        type: Object as PropType<(SortValueFunction | string)>,
        required: false
    },

    /**
     * Enabled filtering of this column and specifies what type of filtering
     * will be done.
     */
    filter: {
        type: Object as PropType<GridColumnFilter>,
        required: false
    },

    /**
     * Specifies how to get the value to use when filtering by this column.
     * This is used on combination with the `filter` setting only. If a
     * function is be provided then it will be called with the row and the
     * column definition and must return a value recognized by the filter.
     * If a string is provided then it will be used as a Lava Template which
     * will be passed the `row` object used to calculate the value.
     */
    filterValue: {
        type: Object as PropType<(FilterValueFunction | string)>,
        required: false
    },

    /**
     * Specifies how to determine the unique value for cells in this column.
     * This is used by some operations to find only the distinct values of
     * the entire dataset. If specified then the function will be passed the
     * row and column definition and must return either a string, number or
     * undefined. Otherwise the value of the row in `field` will be used to
     * determine this value.
     */
    uniqueValue: {
        type: Object as PropType<UniqueValueFunction>,
        required: false
    },

    /**
     * Provides a custom component that will be used to format and display
     * the cell. This is rarely needed as you can usually accomplish the same
     * with a template that defines the body content.
     */
    format: {
        type: Object as PropType<VNode>,
        required: false
    }
};

export const standardCellProps: StandardCellProps = {
    column: {
        type: Object as PropType<GridColumnDefinition>,
        required: true
    },
    row: {
        type: Object as PropType<Record<string, unknown>>,
        required: true
    }
};

export const standardFilterProps: StandardFilterProps = {
    modelValue: {
        type: Object as PropType<unknown>,
        required: false
    },

    column: {
        type: Object as PropType<GridColumnDefinition>,
        required: true
    },

    grid: {
        type: Object as PropType<IGridState>,
        required: true
    }
};

// #endregion

// #region Filter Matches Functions

export function textFilterMatches(needle: unknown, haystack: unknown): boolean {
    if (typeof (needle) !== "string") {
        return false;
    }

    if (!needle) {
        return true;
    }

    const lowerNeedle = needle.toLowerCase();

    if (haystack && typeof (haystack) === "string") {
        return haystack.toLowerCase().includes(lowerNeedle);
    }

    return false;
}

export function pickExistingFilterMatches(needle: unknown, haystack: unknown): boolean {
    if (!Array.isArray(needle)) {
        return false;
    }

    if (needle.length === 0) {
        return true;
    }

    return needle.some(n => deepEqual(n, haystack, true));
}

export function numberFilterMatches(needle: unknown, haystack: unknown, column: GridColumnDefinition, grid: IGridState): boolean {
    if (!needle || typeof needle !== "object") {
        return false;
    }

    // Allow undefined values and number values, but everything else is
    // considered a non-match.
    if (haystack !== undefined && typeof haystack !== "number") {
        return false;
    }

    if (needle["method"] === NumberFilterMethod.Equals) {
        return haystack === needle["value"];
    }
    else if (needle["method"] === NumberFilterMethod.DoesNotEqual) {
        return haystack !== needle["value"];
    }

    // All the remaining comparison types require a value.
    if (haystack === undefined) {
        return false;
    }

    if (needle["method"] === NumberFilterMethod.GreaterThan) {
        return haystack > needle["value"];
    }
    else if (needle["method"] === NumberFilterMethod.GreaterThanOrEqual) {
        return haystack >= needle["value"];
    }
    else if (needle["method"] === NumberFilterMethod.LessThan) {
        return haystack < needle["value"];
    }
    else if (needle["method"] === NumberFilterMethod.LessThanOrEqual) {
        return haystack <= needle["value"];
    }
    else if (needle["method"] === NumberFilterMethod.Between) {
        if (typeof needle["value"] !== "number" || typeof needle["secondValue"] !== "number") {
            return false;
        }

        return haystack >= needle["value"] && haystack <= needle["secondValue"];
    }
    else if (needle["method"] === NumberFilterMethod.TopN) {
        const nCount = needle["value"];

        if (typeof nCount !== "number" || nCount <= 0) {
            return false;
        }

        const cacheKey = `number-filter-${column.name}.top-${nCount}`;
        let topn = grid.cache[cacheKey] as number | undefined;

        if (topn === undefined) {
            topn = calculateColumnTopNRowValue(grid.rows, nCount, column, grid);
            grid.cache[cacheKey] = topn;
        }

        return haystack >= topn;
    }
    else if (needle["method"] === NumberFilterMethod.AboveAverage) {
        const cacheKey = `number-filter-${column.name}.average`;
        let average = grid.cache[cacheKey] as number | undefined;

        if (average === undefined) {
            average = calculateColumnAverageValue(grid.rows, column, grid);
            grid.cache[cacheKey] = average;
        }

        return haystack > average;
    }
    else if (needle["method"] === NumberFilterMethod.BelowAverage) {
        const cacheKey = `number-filter-${column.name}.average`;
        let average = grid.cache[cacheKey] as number | undefined;

        if (average === undefined) {
            average = calculateColumnAverageValue(grid.rows, column, grid);
            grid.cache[cacheKey] = average;
        }

        return haystack < average;
    }
    else {
        return false;
    }
}

// #endregion

// #region Functions

export function calculateColumnAverageValue(rows: Record<string, unknown>[], column: GridColumnDefinition, grid: IGridState): number {
    let count = 0;
    let total = 0;
    for (const row of rows) {
        const rowValue = column.filterValue(row, column, grid);

        if (typeof rowValue === "number") {
            total += rowValue;
            count++;
        }
    }

    return count === 0 ? 0 : total / count;
}

export function calculateColumnTopNRowValue(rows: Record<string, unknown>[], rowCount: number, column: GridColumnDefinition, grid: IGridState): number {
    const values: number[] = [];

    for (const row of rows) {
        const rowValue = column.filterValue(row, column, grid);

        if (typeof rowValue === "number") {
            values.push(rowValue);
        }
    }

    // Sort in descending order.
    values.sort((a, b) => b - a);

    if (rowCount <= values.length) {
        return values[rowCount - 1];
    }
    else {
        return values[values.length - 1];
    }
}

function getOrAddRowCacheValue<T>(row: Record<string, unknown>, column: GridColumnDefinition, key: string, gridState: IGridState, factory: ((row: Record<string, unknown>, column: GridColumnDefinition) => T)): T {
    return gridState.rowCache.getOrAdd<T>(row, `${column.name}-${key}`, () => factory(row, column));
}

export function getColumnDefinitions(columnNodes: VNode[]): GridColumnDefinition[] {
    const columns: GridColumnDefinition[] = [];

    for (const node of columnNodes) {
        const name = getVNodeProp<string>(node, "name");

        if (!name) {
            if (getVNodeProp<boolean>(node, "__attributeColumns") !== true) {
                continue;
            }

            const attributes = getVNodeProp<AttributeFieldDefinitionBag[]>(node, "attributes");
            if (!attributes) {
                continue;
            }

            for (const attribute of attributes) {
                if (!attribute.name) {
                    continue;
                }

                columns.push({
                    name: attribute.name,
                    title: attribute.title ?? undefined,
                    field: attribute.name,
                    uniqueValue: (r, c) => c.field ? String(r[c.field]) : "",
                    sortValue: (r, c) => c.field ? String(r[c.field]) : undefined,
                    quickFilterValue: (r, c, g) => getOrAddRowCacheValue(r, c, "quickFilterValue", g, () => c.field ? String(r[c.field]) : undefined),
                    filterValue: (r, c) => c.field ? String(r[c.field]) : undefined,
                    format: getVNodeProp<VNode>(node, "format") ?? defaultCell,
                    props: {},
                    cache: new GridCache()
                });
            }

            continue;
        }

        const field = getVNodeProp<string>(node, "field");
        let sortValue = getVNodeProp<ValueFormatterFunction | string>(node, "sortValue");

        if (!sortValue) {
            const sortField = getVNodeProp<string>(node, "sortField") || field;

            sortValue = sortField ? (r) => String(r[sortField]) : undefined;
        }
        else if (typeof sortValue === "string") {
            const template = sortValue;

            sortValue = (row): string | undefined => {
                return resolveMergeFields(template, { row });
            };
        }

        let quickFilterValue = getVNodeProp<((row: Record<string, unknown>, column: GridColumnDefinition) => string | undefined)>(node, "quickFilterValue");

        if (!quickFilterValue) {
            quickFilterValue = (r, c): string | undefined => {
                if (!c.field) {
                    return undefined;
                }

                const v = r[c.field];

                if (typeof v === "string") {
                    return v;
                }
                else if (typeof v === "number") {
                    return v.toString();
                }
                else {
                    return undefined;
                }
            };
        }
        else if (typeof quickFilterValue === "string") {
            const template = quickFilterValue;

            quickFilterValue = (row): string | undefined => {
                return resolveMergeFields(template, { row });
            };
        }

        let filterValue = getVNodeProp<FilterValueFunction | string>(node, "filterValue");

        if (filterValue === undefined) {
            filterValue = (r, c): unknown => {
                if (!c.field) {
                    return undefined;
                }

                return r[c.field];
            };
        }
        else if (typeof filterValue === "string") {
            const template = filterValue;

            filterValue = (row): unknown => {
                return resolveMergeFields(template, { row });
            };
        }

        let uniqueValue = getVNodeProp<UniqueValueFunction>(node, "uniqueValue");

        if (!uniqueValue) {
            uniqueValue = (r, c) => {
                if (!c.field || r[c.field] === undefined) {
                    return undefined;
                }

                const v = r[c.field];

                if (typeof v === "string" || typeof v === "number") {
                    return v;
                }

                return JSON.stringify(v);
            };
        }

        columns.push({
            name,
            title: getVNodeProp<string>(node, "title"),
            field,
            format: node.children?.["body"] ?? getVNodeProp<VNode>(node, "format") ?? defaultCell,
            filter: getVNodeProp<GridColumnFilter>(node, "filter"),
            uniqueValue,
            sortValue,
            filterValue,
            quickFilterValue: (r, c, g) => quickFilterValue !== undefined ? getOrAddRowCacheValue(r, c, "quickFilterValue", g, quickFilterValue) : undefined,
            props: getVNodeProps(node),
            cache: new GridCache()
        });
    }

    return columns;
}

/**
 * Gets the key to use on the internal cache object to load the cached data
 * for the specified row.
 *
 * @param row The row whose identifier key is needed.
 *
 * @returns The identifier key of the row or `undefined` if it could not be determined.
 */
export function getRowKey(row: Record<string, unknown>, itemIdKey?: string): string | undefined {
    if (!itemIdKey) {
        return undefined;
    }

    const rowKey = row[itemIdKey];

    if (typeof rowKey === "string") {
        return rowKey;
    }
    else if (typeof rowKey === "number") {
        return `${rowKey}`;
    }
    else {
        return undefined;
    }
}

// #endregion

// #region Classes

/**
 * Default implementation used for caching data with Grid.
 *
 * @private This class is meant for internal use only.
 */
export class GridCache implements IGridCache {
    /** The private cache data storage. */
    private cacheData: Record<string, unknown> = {};

    public clear(): void {
        this.cacheData = {};
    }

    public remove(key: string): void {
        if (key in this.cacheData) {
            delete this.cacheData[key];
        }
    }

    public get<T = unknown>(key: string): T | undefined {
        if (key in this.cacheData) {
            return <T>this.cacheData[key];
        }
        else {
            return undefined;
        }
    }

    public getOrAdd<T = unknown>(key: string, factory: () => T): T;
    public getOrAdd<T = unknown>(key: string, factory: () => T | undefined): T | undefined;
    public getOrAdd<T = unknown>(key: string, factory: () => T | undefined): T | undefined {
        if (key in this.cacheData) {
            return <T>this.cacheData[key];
        }
        else {
            const value = factory();

            if (value !== undefined) {
                this.cacheData[key] = value;
            }

            return value;
        }
    }

    public addOrReplace<T = unknown>(key: string, value: T): T {
        this.cacheData[key] = value;

        return value;
    }
}

/**
 * Default implementation used for caching grid row data.
 *
 * @private This class is meant for internal use only.
 */
export class GridRowCache implements IGridRowCache {
    /** The internal cache object used to find the cached row data. */
    private cache: IGridCache = new GridCache();

    /** The key name to use on the row objects to find the row identifier. */
    private rowItemKey?: string;

    /**
     * Creates a new grid row cache object that provides caching for each row.
     * This is used by other parts of the grid to cache expensive calculations
     * that pertain to a single row.
     *
     * @param itemIdKey The key name to use on the row objects to find the row identifier.
     */
    public constructor(itemIdKey: string | undefined) {
        this.rowItemKey = itemIdKey;
    }

    /**
     * Gets the key to use on the internal cache object to load the cached data
     * for the specified row.
     *
     * @param row The row whose identifier key is needed.
     *
     * @returns The identifier key of the row or `undefined` if it could not be determined.
     */
    private getRowKey(row: Record<string, unknown>): string | undefined {
        return getRowKey(row, this.rowItemKey);
    }

    /**
     * Sets the key that will be used when accessing a row to determine its
     * unique identifier in the grid. This will also clear all cached data.
     *
     * @param itemKey The key name to use on the row objects to find the row identifier.
     */
    public setRowItemKey(itemKey: string | undefined): void {
        if (this.rowItemKey !== itemKey) {
            this.rowItemKey = itemKey;
            this.clear();
        }
    }

    public clear(): void {
        this.cache.clear();
    }

    public remove(row: Record<string, unknown>): void;
    public remove(row: Record<string, unknown>, key: string): void;
    public remove(row: Record<string, unknown>, key?: string): void {
        const rowKey = this.getRowKey(row);

        if (!rowKey) {
            return;
        }

        const cacheRow = this.cache.get<GridCache>(rowKey);

        if (!cacheRow) {
            return;
        }

        if (!key) {
            cacheRow.clear();
        }
        else {
            cacheRow.remove(key);
        }
    }

    public get<T = unknown>(row: Record<string, unknown>, key: string): T | undefined {
        const rowKey = this.getRowKey(row);

        if (!rowKey) {
            return undefined;
        }

        return this.cache.getOrAdd(rowKey, () => new GridCache()).get<T>(key);
    }

    public getOrAdd<T = unknown>(row: Record<string, unknown>, key: string, factory: () => T): T;
    public getOrAdd<T = unknown>(row: Record<string, unknown>, key: string, factory: () => T | undefined): T | undefined;
    public getOrAdd<T = unknown>(row: Record<string, unknown>, key: string, factory: () => T | undefined): T | undefined {
        const rowKey = this.getRowKey(row);

        if (!rowKey) {
            return factory();
        }

        return this.cache.getOrAdd(rowKey, () => new GridCache()).getOrAdd<T>(key, factory);
    }

    public addOrReplace<T = unknown>(row: Record<string, unknown>, key: string, value: T): T {
        const rowKey = this.getRowKey(row);

        if (!rowKey) {
            return value;
        }

        return this.cache.getOrAdd(rowKey, () => new GridCache()).addOrReplace<T>(key, value);
    }
}

export class GridState implements IGridState {
    // #region Properties

    private internalRows: ShallowRef<Record<string, unknown>[]> = shallowRef([]);

    /** This tracks the state of each row when operating in reactive mode. */
    private rowReactiveTracker: Record<string, string> = {};

    /** The handle that can be used to stop watching row changes. */
    private internalRowsWatcher?: WatchStopHandle;

    /** Determines if we are monitoring for changes to the row data. */
    private liveUpdates: boolean;

    private itemKey?: string;

    private quickFilter: string = "";

    private columnFilters: Record<string, unknown | undefined> = {};

    private columnSort?: ColumnSort;

    public readonly filteredRows: ShallowRef<Record<string, unknown>[]> = shallowRef([]);

    public readonly sortedRows: ShallowRef<Record<string, unknown>[]> = shallowRef([]);

    public readonly visibleRows: ShallowRef<Record<string, unknown>[]> = shallowRef([]);

    public readonly columns: GridColumnDefinition[];

    public visibleColumns: GridColumnDefinition[];

    public cache: IGridCache = new GridCache();

    public rowCache: IGridRowCache;

    // #endregion

    // #region Constructors

    constructor(columns: GridColumnDefinition[], liveUpdates: boolean) {
        this.rowCache = new GridRowCache(undefined);
        this.columns = columns;
        this.visibleColumns = columns;
        this.liveUpdates = liveUpdates;
    }

    /**
     * Dispose of all resources this grid state has. This includes any watchers
     * and other things that might need to be manually destroyed to free up
     * memory. A common pattern would be to call this in the onUmounted() callback.
     */
    public dispose(): void {
        if (this.internalRowsWatcher) {
            this.internalRowsWatcher();
            this.internalRowsWatcher = undefined;
        }
    }

    // #endregion

    // #region Property Accessors

    get rows(): Record<string, unknown>[] {
        return this.internalRows.value;
    }

    // #endregion

    private initializeReactiveTracker(): void {
        const rows = unref(this.internalRows);

        this.rowReactiveTracker = {};

        for (let i = 0; i < rows.length; i++) {
            const key = getRowKey(rows[i], this.itemKey);

            if (key) {
                this.rowReactiveTracker[key] = JSON.stringify(rows[i]);
            }
        }
    }

    private detectRowChanges(): void {
        const rows = unref(this.internalRows);
        const knownKeys: string[] = [];

        // Loop through all the rows we still have and check for any that
        // are new or have been modified.
        for (let i = 0; i < rows.length; i++) {
            const key = getRowKey(rows[i], this.itemKey);

            if (!key) {
                continue;
            }

            // Save the key for later.
            knownKeys.push(key);

            if (!this.rowReactiveTracker[key]) {
                console.log("Row added", rows[i]);
            }
            else if (this.rowReactiveTracker[key] !== JSON.stringify(rows[i])) {
                console.log("Row updated", rows[i]);
                this.rowReactiveTracker[key] = JSON.stringify(rows[i]);
                this.rowCache.remove(rows[i]);
            }
        }

        // Loop through all the row key values that are being tracked and
        // see if any no longer exist in our data set.
        const oldKeys = Object.keys(this.rowReactiveTracker);
        for (let i = 0; i < oldKeys.length; i++) {
            if (!knownKeys.includes(oldKeys[i])) {
                console.log("Removed row id", oldKeys[i]);
                // TODO: Remove the cache data for a row by it's key.
                delete this.rowReactiveTracker[oldKeys[i]];
            }
        }
    }

    public setItemKey(value: string | undefined): void {
        this.itemKey = value;
        (this.rowCache as GridRowCache).setRowItemKey(value);
    }

    public setDataRows(rows: Record<string, unknown>[]): void {
        // Stop watching the old rows if we are currently watching for changes.
        if (this.internalRowsWatcher) {
            this.internalRowsWatcher();
            this.internalRowsWatcher = undefined;
        }

        // Update out internal rows and clear all the cache.
        this.internalRows.value = rows;
        this.cache.clear();
        this.rowCache.clear();

        // Start watching for changes if we are reactive.
        if (this.liveUpdates) {
            this.initializeReactiveTracker();

            this.internalRowsWatcher = watch(() => rows, () => {
                this.detectRowChanges();
                this.updateFilteredRows();
            }, { deep: true });
        }

        this.updateFilteredRows();
    }

    public setFilters(quickFilter: string | undefined, columnFilters: Record<string, unknown> | undefined): void {
        this.quickFilter = quickFilter ?? "";
        this.columnFilters = columnFilters ?? {};
        this.updateFilteredRows();
    }

    public setSort(columnSort: ColumnSort | undefined): void {
        this.columnSort = columnSort;
        this.updateSortedRows();
    }

    private updateFilteredRows(): void {
        const start = Date.now();
        if (this.columns.length > 0) {
            const columns = this.visibleColumns;
            const quickFilterRawValue = this.quickFilter.toLowerCase();

            const result = this.rows.filter(row => {
                const quickFilterMatch = !quickFilterRawValue || columns.some((column): boolean => {
                    const value = column.quickFilterValue(row, column, this);

                    if (value === undefined) {
                        return false;
                    }

                    return value.toLowerCase().includes(quickFilterRawValue);
                });

                const filtersMatch = columns.every(column => {
                    if (!column.filter) {
                        return true;
                    }

                    const columnFilterValue = this.columnFilters[column.name];

                    if (columnFilterValue === undefined) {
                        return true;
                    }

                    const value: unknown = column.filterValue(row, column, this);

                    if (value === undefined) {
                        return false;
                    }

                    return column.filter.matches(columnFilterValue, value, column, this);
                });

                return quickFilterMatch && filtersMatch;
            });

            this.filteredRows.value = result;
        }
        else {
            this.filteredRows.value = [];
        }

        console.log(`Filtering took ${Date.now() - start}ms.`);

        this.updateSortedRows();
    }

    private updateSortedRows(): void {
        const columnSort = this.columnSort;

        if (!columnSort) {
            this.sortedRows.value = this.filteredRows.value;

            return;
        }

        const start = Date.now();
        const column = this.visibleColumns.find(c => c.name === columnSort.column);
        const order = columnSort.isDescending ? -1 : 1;

        if (!column) {
            throw new Error("Invalid sort definition");
        }

        const sortValue = column.sortValue;

        // Pre-process each row to calculate the sort value. Otherwise it will
        // be calculated exponentially during sort. This provides a serious
        // performance boost when sorting Lava columns.
        const rows = this.filteredRows.value.map(r => {
            let value: string | number | undefined;

            if (sortValue) {
                value = sortValue(r, column, this);
            }
            else {
                value = undefined;
            }

            return {
                row: r,
                value
            };
        });

        rows.sort((a, b) => {
            if (a.value === undefined) {
                return -order;
            }
            else if (b.value === undefined) {
                return order;
            }
            else if (a.value < b.value) {
                return -order;
            }
            else if (a.value > b.value) {
                return order;
            }
            else {
                return 0;
            }
        });

        this.sortedRows.value = rows.map(r => r.row);

        console.log(`sortedRows took ${Date.now() - start}ms.`);
    }
}

// #endregion

// #region Internal Components

/**
 * This is a special cell we use when no default format cell has been defined.
 */
const defaultCell = defineComponent({
    props: standardCellProps,

    setup(props) {
        return () => props.column.field ? props.row[props.column.field] : "";
    }
});

// #endregion

