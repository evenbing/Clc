
(function() {        
    $(function() {
        finput.style = 0;
        abp.services.app.work.getMyCheckinAffair().done(function (wk) {
            work.me = wk;
            if (!work.validate2()) return;
            $('#dd').datebox('setValue', work.me.today);
            abp.services.app.work.getTodaySameWpAffairs(work.me.workplaceId, work.me.today, work.me.depotId).done(function (data) {
                $('#prevAffair').combobox({
                    data: data,
                    valueField: 'value',
                    textField: 'displayText'
                });

                $('#prevAffair').combobox('setValue', work.me.affairId);
            });
        });
            
        $('#prevAffair').combobox({
            onChange: function(val) {
                currentAffairId = $('#prevAffair').combobox('getValue');
                $('#dg').datagrid({
                    url: 'GridDataTemp?AffairId=' + currentAffairId
                });
            }
        });

        $('#take').checkbox({
            onChange: function() {
                if ($('#take').checkbox('options').checked) {
                    $('#tb').css("background-color","red");
                    finput.style = 1;
                }
                else {
                    $('#tb').css("background-color","");
                    finput.style = 0;
                }
            }
        });
    });
})();
