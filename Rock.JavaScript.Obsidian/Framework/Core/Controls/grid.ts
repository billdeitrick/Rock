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

import { defineComponent, isReactive, PropType, ref, Ref, shallowRef, ShallowRef, unref, VNode, watch, WatchStopHandle } from "vue";
import { NumberFilterMethod } from "@Obsidian/Enums/Controls/Grid/numberFilterMethod";
import { GridColumnFilter, GridColumnDefinition, IGridState, StandardFilterProps, StandardCellProps, IGridCache, IGridRowCache, ValueFormatterFunction, ColumnSort } from "@Obsidian/Types/Controls/grid";
import { getVNodeProp, getVNodeProps } from "@Obsidian/Utility/component";
import { resolveMergeFields } from "@Obsidian/Utility/lava";
import { deepEqual } from "@Obsidian/Utility/util";
import { AttributeFieldDefinitionBag } from "@Obsidian/ViewModels/Core/Grid/attributeFieldDefinitionBag";

const defaultCell = defineComponent({
    props: {
        column: {
            type: Object as PropType<GridColumnDefinition>,
            required: true
        },
        row: {
            type: Object as PropType<Record<string, unknown>>,
            required: true
        }
    },

    setup(props) {
        return () => props.column.field ? props.row[props.column.field] : "";
    }
});

// #region Standard Component Props

export const standardColumnProps = {
    name: {
        type: String as PropType<string>,
        default: ""
    },

    title: {
        type: String as PropType<string>,
        required: false
    },

    textValue: {
        type: Object as PropType<(((row: Record<string, unknown>, column: GridColumnDefinition) => string | number | undefined) | string)>,
        required: false
    },

    sortField: {
        type: String as PropType<string>,
        required: false
    },

    sortValue: {
        type: Object as PropType<(((row: Record<string, unknown>, column: GridColumnDefinition) => string | number | undefined) | string)>,
        required: false
    },

    filter: {
        type: Object as PropType<GridColumnFilter>,
        required: false
    },

    filterValue: {
        type: Object as PropType<(((row: Record<string, unknown>, column: GridColumnDefinition) => string | number | undefined) | string)>,
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

export function numberFilterMatches(needle: unknown, haystack: unknown, column: GridColumnDefinition, gridData: IGridState): boolean {
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
        let topn = gridData.cache[cacheKey] as number | undefined;

        if (topn === undefined) {
            topn = calculateColumnTopNRowValue(gridData.rows, nCount, column);
            gridData.cache[cacheKey] = topn;
        }

        return haystack >= topn;
    }
    else if (needle["method"] === NumberFilterMethod.AboveAverage) {
        const cacheKey = `number-filter-${column.name}.average`;
        let average = gridData.cache[cacheKey] as number | undefined;

        if (average === undefined) {
            average = calculateColumnAverageValue(gridData.rows, column);
            gridData.cache[cacheKey] = average;
        }

        return haystack > average;
    }
    else if (needle["method"] === NumberFilterMethod.BelowAverage) {
        const cacheKey = `number-filter-${column.name}.average`;
        let average = gridData.cache[cacheKey] as number | undefined;

        if (average === undefined) {
            average = calculateColumnAverageValue(gridData.rows, column);
            gridData.cache[cacheKey] = average;
        }

        return haystack < average;
    }
    else {
        return false;
    }
}

// #endregion

// #region Functions

export function calculateColumnAverageValue(rows: Record<string, unknown>[], column: GridColumnDefinition): number {
    let count = 0;
    let total = 0;
    for (const row of rows) {
        const rowValue = column.filterValue(row, column);

        if (typeof rowValue === "number") {
            total += rowValue;
            count++;
        }
    }

    return count === 0 ? 0 : total / count;
}

export function calculateColumnTopNRowValue(rows: Record<string, unknown>[], rowCount: number, column: GridColumnDefinition): number {
    const values: number[] = [];

    for (const row of rows) {
        const rowValue = column.filterValue(row, column);

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

        let filterValue = getVNodeProp<((row: Record<string, unknown>, column: GridColumnDefinition) => unknown) | string>(node, "filterValue");

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

            filterValue = (row): string | undefined => {
                return resolveMergeFields(template, { row });
            };
        }

        let uniqueValue = getVNodeProp<ValueFormatterFunction>(node, "uniqueValue");

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
    private rowItemIdKey?: string;

    /**
     * Creates a new grid row cache object that provides caching for each row.
     * This is used by other parts of the grid to cache expensive calculations
     * that pertain to a single row.
     *
     * @param itemIdKey The key name to use on the row objects to find the row identifier.
     */
    public constructor(itemIdKey: string | undefined) {
        this.rowItemIdKey = itemIdKey;
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
        return getRowKey(row, this.rowItemIdKey);
    }

    /**
     * Sets the key that will be used when accessing a row to determine its
     * unique identifier in the grid. This will also clear all cached data.
     *
     * @param itemIdKey The key name to use on the row objects to find the row identifier.
     */
    public setRowItemIdKey(itemIdKey: string | undefined): void {
        if (this.rowItemIdKey !== itemIdKey) {
            this.rowItemIdKey = itemIdKey;
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
    private internalRows: ShallowRef<Record<string, unknown>[]> = shallowRef([]);

    private itemIdKey?: string;

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

    constructor(columns: GridColumnDefinition[], itemIdKey: string | undefined) {
        this.rowCache = new GridRowCache(itemIdKey);
        this.columns = columns;
        this.visibleColumns = columns;
        this.itemIdKey = itemIdKey;
    }

    // #region Property Accessors

    get rows(): Record<string, unknown>[] {
        return this.internalRows.value;
    }

    // #endregion

    private rowWatchers: Record<string, WatchStopHandle> = {};

    private updateRowWatchers(): void {
        // Quick test, if we don't have an item id key, then just kill
        // all the watchers.
        if (!this.itemIdKey) {
            Object.values(this.rowWatchers).forEach(wsh => wsh());
            this.rowWatchers = {};
            return;
        }

        const start = Date.now();
        console.log("start reactive check");
        const existingWatcherRowKeys = Object.keys(this.rowWatchers);
        const currentRowKeys: Record<string, boolean> = {};

        // Look for new rows that have been added.
        for (let i = 0; i < this.internalRows.value.length; i++) {
            const row = this.internalRows.value[i];
            const rowKey = getRowKey(row, this.itemIdKey);

            if (!rowKey) {
                continue;
            }

            currentRowKeys[rowKey] = true;

            if (!this.rowWatchers[rowKey]) {
                const watcher = watch(() => row, () => {
                    console.log("Row changed", row);
                    this.rowCache.remove(row);
                    this.updateFilteredRows();
                }, {
                    deep: true
                });

                this.rowWatchers[rowKey] = watcher;
            }
        }
        const m1 = Date.now();

        // Look for old rows that have been removed.
        existingWatcherRowKeys.forEach(rk => {
            if (!currentRowKeys[rk]) {
                console.log("removed watcher for", rk);
                this.rowWatchers[rk]();
                delete this.rowWatchers[rk];
            }
        });
        const stop = Date.now();
        console.log("end reactive check", stop - start, m1 - start);
    }

    public setDataRows(rows: Record<string, unknown>[]): void {
        this.internalRows.value = rows;
        this.cache.clear();
        this.rowCache.clear();
        this.updateRowWatchers();

        watch(() => rows, () => {
            this.updateRowWatchers();
            this.updateFilteredRows();
        }, { deep: true });
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

                    const value: unknown = column.filterValue(row, column);

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
                value = sortValue(r, column);
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
