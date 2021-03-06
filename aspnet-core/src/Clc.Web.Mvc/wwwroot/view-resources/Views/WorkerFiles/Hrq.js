﻿(function() {
    $(function() {
        abp.services.app.field.getComboItems('Depot').done(function (d) {
            $('#depot').combobox({
                data: d, valueField: 'value', textField: 'displayText'
            });
        });
        abp.services.app.type.getComboItems('Post').done(function (d) {
            $('#post').combobox({
                data: d, valueField: 'value', textField: 'displayText'
            });
        });

        abp.services.app.type.getComboItems('Status').done(function (d) {
            $('#status').combobox({
                data: d, valueField: 'value', textField: 'displayText'
            });
        });

        $('#fmSearch').children('a[name="search"]').click(function (e) {
            $('#dg').datagrid({
                url: 'GetPagedData',
                queryParams: $('#fmSearch').serializeFormToObject() // { depotId: $('#depot').val(), postId: $('#post').val() }
            });
        });

        $('#dg').datagrid({
            onSelect: function() {
                var row = $('#dg').datagrid('getSelected');
                $('#fm').form('load', row);
            }
        })
    });
})();