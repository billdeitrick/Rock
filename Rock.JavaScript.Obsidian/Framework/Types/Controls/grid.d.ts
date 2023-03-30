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

import { Component, PropType, ShallowRef, VNode } from "vue";

/**
 * Defines a generic grid cache object. This can be used to store and get
 * data from a cache. The cache is unique to the grid instance so there is
 * no concern of multiple grids conflicting.
 */
export interface IGridCache {
    /**
     * Removes all values from the cache.
     */
    clear(): void;

    /**
     * Removes a single item from the cache.
     *
     * @param key The identifier of the value to be removed from the cache.
     */
    remove(key: string): void;

    /**
     * Gets an existing value from the cache.
     *
     * @param key The identifier of the value.
     *
     * @returns The value found in the cache or undefined if it was not found.
     */
    get<T = unknown>(key: string): T | undefined;

    /**
     * Gets an existing value from cache or adds it into the cache.
     *
     * @param key The identifier of the cached value.
     * @param factory The function to call when adding the value.
     *
     * @returns The existing value or the newly created value.
     */
    getOrAdd<T = unknown>(key: string, factory: () => T): T;

    /**
     * Gets an existing value form cache or adds it into the cache.
     *
     * @param key The identifier of the cached value.
     * @param factory The function to call when adding the value. If undefined is returned then the value is not added to the cache.
     *
     * @returns The existing value or the newly created value. Returns undefined if it could not be found or created.
     */
    getOrAdd<T = unknown>(key: string, factory: () => T | undefined): T | undefined;

    /**
     * Adds the value if it does not exist in cache or replaces the existing
     * value in cache with the new value.
     *
     * @param key The identifier of the cached value.
     * @param value The value that should be placed into the cache.
     *
     * @returns The value that was placed into the cache.
     */
    addOrReplace<T = unknown>(key: string, value: T): T;
}

/**
 * Defines a grid cache object used for row data. This can be used to store and
 * get data from cache for a specific row. The cache is unique to the grid
 * instance so there is no concern of multiple grids conflicting.
 */
export interface IGridRowCache {
    /**
     * Removes all values for all rows from the cache.
     */
    clear(): void;

    /**
     * Removes all the cached values for the specified row.
     *
     * @param row The row whose cached values should be removed.
     */
    remove(row: Record<string, unknown>): void;

    /**
     * Removes a single item from the cache.
     *
     * @param row The row whose cached key value should be removed.
     * @param key The identifier of the value to be removed from the row cache.
     */
    remove(row: Record<string, unknown>, key: string): void;

    /**
     * Gets an existing value from the cache.
     *
     * @param row The row whose cached key value should be retrieved.
     * @param key The identifier of the value.
     *
     * @returns The value found in the cache or undefined if it was not found.
     */
    get<T = unknown>(row: Record<string, unknown>, key: string): T | undefined;

    /**
     * Gets an existing value from cache or adds it into the cache.
     *
     * @param row The row whose cached key value should be retrieved.
     * @param key The identifier of the cached value.
     * @param factory The function to call when adding the value.
     *
     * @returns The existing value or the newly created value.
     */
    getOrAdd<T = unknown>(row: Record<string, unknown>, key: string, factory: () => T): T;

    /**
     * Gets an existing value form cache or adds it into the cache.
     *
     * @param row The row whose cached key value should be retrieved.
     * @param key The identifier of the cached value.
     * @param factory The function to call when adding the value. If undefined is returned then the value is not added to the cache.
     *
     * @returns The existing value or the newly created value. Returns undefined if it could not be found or created.
     */
    getOrAdd<T = unknown>(row: Record<string, unknown>, key: string, factory: () => T | undefined): T | undefined;

    /**
     * Adds the value if it does not exist in cache or replaces the existing
     * value in cache with the new value.
     *
     * @param row The row whose cached key value should be retrieved.
     * @param key The identifier of the cached value.
     * @param value The value that should be added into the cache.
     *
     * @returns The value that was placed into the cache.
     */
    addOrReplace<T = unknown>(row: Record<string, unknown>, key: string, value: T): T;
}

export interface IGridState {
    readonly cache: IGridCache;

    readonly rowCache: IGridRowCache;

    readonly columns: GridColumnDefinition[];

    readonly rows: Record<string, unknown>[];

    readonly filteredRows: ShallowRef<Record<string, unknown>[]>;

    readonly sortedRows: ShallowRef<Record<string, unknown>[]>;

    readonly visibleRows: ShallowRef<Record<string, unknown>[]>;

    setDataRows(rows: Record<string, unknown>[]): void;
}

/** A function that will be called in response to an action. */
export type GridActionCallback = (event: Event) => void | Promise<void>;

/**
 * A function that will be called in order to determine if a row matches the
 * filtering request for the column.
 */
export type GridColumnFilterMatchesCallback = (needle: unknown, haystack: unknown, column: GridColumnDefinition, gridData: IGridState) => boolean;

export type SortValueFunction = (row: Record<string, unknown>, column: GridColumnDefinition, grid: IGridState) => string | number | undefined;

export type FilterValueFunction = (row: Record<string, unknown>, column: GridColumnDefinition, grid: IGridState) => unknown | undefined;

export type QuickFilterValueFunction = (row: Record<string, unknown>, column: GridColumnDefinition, grid: IGridState) => string | undefined;

export type ValueFormatterFunction = (row: Record<string, unknown>, column: GridColumnDefinition, grid: IGridState) => string | number | undefined;

export type UniqueValueFunction = (row: Record<string, unknown>, column: GridColumnDefinition, grid: IGridState) => string | number | undefined;

export type StandardCellProps = {
    column: {
        type: PropType<GridColumnDefinition>,
        required: true
    },

    row: {
        type: PropType<Record<string, unknown>>,
        required: true
    }
};

export type StandardFilterProps = {
    modelValue: {
        type: PropType<unknown>,
        required: false
    },

    column: {
        type: PropType<GridColumnDefinition>,
        required: true
    },

    grid: {
        type: PropType<IGridState>,
        required: true
    }
};

/** Defines a single action related to a Grid control. */
export type GridAction = {
    /**
     * The title of the action, this should be a very short (one or two words)
     * description of the action that will be performed, such as "Delete".
     */
    title?: string;

    /**
     * The tooltip to display for this action.
     */
    tooltip?: string;

    /**
     * The CSS class for the icon used when displaying this action.
     */
    iconCssClass?: string;

    /** The callback function that will handle the action. */
    handler?: GridActionCallback;

    /** If true then the action will be disabled and not respond to clicks. */
    disabled?: boolean;

    /** True if the action is currently executing. */
    executing: boolean;
};

export type GridColumnDefinition = {
    /** The unique name of this column. */
    name: string;

    /** The title to display in the column header. */
    title?: string;

    /** The name of the field in the row object. */
    field?: string;

    /**
     * Formats the value for display in the cell. Should return HTML safe
     * content, meaning if you intend to display the < character you need
     * to HTML encode it as &lt;.
     */
    format: VNode | Component;

    /** Gets the value to use when filtering on the quick filter. */
    quickFilterValue: QuickFilterValueFunction;

    /**
     * Gets the unique value representation of the cell value. For example,
     * a Person column might return the person Guid.
     */
    uniqueValue: ValueFormatterFunction;

    /** Gets the value to use when sorting. */
    sortValue?: ValueFormatterFunction;

    /** Gets the value to use when filtering. */
    filterValue: FilterValueFunction;

    filter?: GridColumnFilter;

    props: Record<string, unknown>;

    cache: IGridCache;
};

export type GridColumnFilter = {
    component: Component;

    matches: GridColumnFilterMatchesCallback;
};

export type FilterComponentProps = {
    modelValue: {
        type: PropType<unknown>,
        required: false
    },

    column: {
        type: PropType<GridColumnDefinition>,
        required: true
    },

    rows: {
        type: PropType<Record<string, unknown>[]>,
        required: true
    }
};

export type ColumnSort = {
    column: string;

    isDescending: boolean;
};
