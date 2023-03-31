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

import Grid from "./Grid/grid.partial.obs";

import AttributeColumns from "./Grid/Columns/attributeColumns.partial";
import BooleanColumn from "./Grid/Columns/booleanColumn.partial";
import Column from "./Grid/Columns/column.partial";
import DateColumn from "./Grid/Columns/dateColumn.partial";
import LabelColumn from "./Grid/Columns/labelColumn.partial";
import NumberColumn from "./Grid/Columns/numberColumn.partial";

import BooleanCell from "./Grid/Cells/booleanCell.partial.obs";
import DateCell from "./Grid/Cells/dateCell.partial.obs";
import NumberCell from "./Grid/Cells/numberCell.partial.obs";
import LabelCell from "./Grid/Cells/labelCell.partial.obs";
import TextCell from "./Grid/Cells/textCell.partial";

import DateFilter from "./Grid/Filters/dateFilter.partial.obs";
import NumberFilter from "./Grid/Filters/numberFilter.partial.obs";
import PickExistingFilter from "./Grid/Filters/pickExistingFilter.partial.obs";
import TextFilter from "./Grid/Filters/textFilter.partial.obs";

import { dateFilterMatches, numberFilterMatches, pickExistingFilterMatches, textFilterMatches } from "@Obsidian/Core/Controls/grid";
import { ColumnFilter } from "@Obsidian/Types/Controls/grid";

// Export main Grid component.
export default Grid;

// Export column components.
export {
    AttributeColumns,
    BooleanColumn,
    Column,
    DateColumn,
    LabelColumn,
    NumberColumn
};

// Export cell components.
export {
    BooleanCell,
    DateCell,
    LabelCell,
    NumberCell,
    TextCell
};

// Export filter components.
export {
    DateFilter,
    NumberFilter,
    PickExistingFilter,
    TextFilter
};

export const dateValueFilter: ColumnFilter = {
    component: DateFilter,

    matches: dateFilterMatches
};

export const numberValueFilter: ColumnFilter = {
    component: NumberFilter,

    matches: numberFilterMatches
};

export const textValueFilter: ColumnFilter = {
     component: TextFilter,

     matches: textFilterMatches
};

export const pickExistingValueFilter: ColumnFilter = {
    component: PickExistingFilter,

    matches: pickExistingFilterMatches
};
