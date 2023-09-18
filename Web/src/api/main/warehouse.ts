import request from '/@/utils/request';
enum Api {
  Addwarehouse = '/api/warehouse/add',
  Deletewarehouse = '/api/warehouse/delete',
  Updatewarehouse = '/api/warehouse/update',
  Pagewarehouse = '/api/warehouse/page',
}

// 增加warehousecs
export const addwarehouse = (params?: any) =>
	request({
		url: Api.Addwarehouse,
		method: 'post',
		data: params,
	});

// 删除warehousecs
export const deletewarehouse = (params?: any) => 
	request({
			url: Api.Deletewarehouse,
			method: 'post',
			data: params,
		});

// 编辑warehousecs
export const updatewarehouse = (params?: any) => 
	request({
			url: Api.Updatewarehouse,
			method: 'post',
			data: params,
		});

// 分页查询warehousecs
export const pagewarehouse = (params?: any) => 
	request({
			url: Api.Pagewarehouse,
			method: 'post',
			data: params,
		});


