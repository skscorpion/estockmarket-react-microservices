import React, { useEffect, useState } from 'react';
import { connect } from 'react-redux';
import axios from 'axios';
import { AgGridReact } from 'ag-grid-react';
import CellButton from './helpers/CellButton';
import 'ag-grid-community/dist/styles/ag-grid.css';
import 'ag-grid-community/dist/styles/ag-theme-alpine.css';
import Box from '@material-ui/core/Box';
import { selectedCompany, setCompanies, fetchCompanies } from '../actions';
import Layout from './layout';
import Authenticate from './helpers/Authenticate';
import Snackbar from '@material-ui/core/Snackbar';
import MuiAlert, { AlertProps } from '@material-ui/lab/Alert';

const Home = (props: any) => {
	const [state, setState] = useState({
		CompanyList: [],
		error: null,
	});
	const [loading, setLoading] = useState(true);

	function Alert(props: AlertProps) {
		return <MuiAlert elevation={6} variant='filled' {...props} />;
	}
	const [open, setOpen] = React.useState(false);
	const [toastrmsg, setMsg] = React.useState('');
	const handleClick = () => {
		setOpen(true);
	};
	const handleClose = (event?: React.SyntheticEvent, reason?: string) => {
		if (reason === 'clickaway') {
			return;
		}

		setOpen(false);
	};
	useEffect((): any => {
		// props.fetchCompanies();
		setState({
			CompanyList: props.Companies,
			error: null,
		});
		let isSubscribed = true;
		const promise = axios.get('http://localhost:8000/api/v1/Company');
		promise
			.then((value) => {
				if (isSubscribed) {
					setState({
						CompanyList: value.data,
						error: null,
					});
				}
				props.setCompanies(value.data);
				setLoading(false);
			})
			.catch((error) => {
				setMsg('Something went wrong, Please Refresh the page');
				setOpen(true);
				if (isSubscribed) {
					setState({
						CompanyList: [],
						error: error,
					});
				}
				setLoading(false);
			});

		return () => (isSubscribed = false);
	}, []);

	const onEditClicked = (id: string) => {
		console.log('id',id)
		props.selectedCompany(id);
		props.history.push('/Edit');
	};

	const onViewClicked = (id: string) => {
		props.selectedCompany(id);
		props.history.push('/View');
	};

	const onDeleteClicked = (id: string) => {
		console.log('######id',id);
		const promise = axios.delete(`http://localhost:8000/api/v1/Company?code=${id}`);
		promise
			.then((value) => {
				setLoading(true);
				setMsg('Company Deleted');
				setOpen(true);
				const promise = axios.get('http://localhost:8000/api/v1/Company');
				promise
					.then((value) => {
						setLoading(false);
						setTimeout(() => {
							setOpen(false);
						}, 2000);
						console.log('value',value)
						setState({
							CompanyList: value.data,
							error: null,
						});
					})
					.catch((error) => {
						setLoading(false);
						setMsg('Something went wrong, Please Refresh the page');
						setOpen(true);
					});
			})
			.catch((error) =>{});
	};

	const gridData: any = {
		columnDefs: [
			{ headerName: 'Company Code', field: 'code', sortable: true },
			{ headerName: 'Company Name', field: 'name', sortable: true },
			{ headerName: 'Stock Exchange', field: 'stockExchange' },
			{
				headerName: 'Action',
				field: 'code',
				width: '100',
				cellRenderer: 'btnCellRenderer',
				cellRendererParams: {
					clicked: (field: any) => onEditClicked(field),
					caption: 'Edit',
					color: 'primary',
				},
			},
			{
				headerName: '',
				field: 'code',
				width: '120',
				cellRenderer: 'btnCellRenderer',
				cellRendererParams: {
					clicked: (field: any) => onViewClicked(field),
					caption: 'Detail',
					color: 'primary',
				},
			},
			{
				headerName: '',
				field: 'code',
				width: '120',
				cellRenderer: 'btnCellRenderer',
				cellRendererParams: {
					clicked: (field: any) => onDeleteClicked(field),
					caption: 'Delete',
					color: 'secondary',
				},
			},
		],
		frameworkComponents: {
			btnCellRenderer: CellButton,
		},
	};

	return (
		<React.Fragment>
			<Layout props={props} title='Home'>
				<Box display='flex' justifyContent='center'>
					<div
						className='ag-theme-alpine'
						style={{ height: '520px', width: '70vw', marginTop: '20px' }}
					>
						<AgGridReact
							columnDefs={gridData.columnDefs}
							rowData={state.CompanyList}
							frameworkComponents={gridData.frameworkComponents}
							pagination={true}
							paginationPageSize={10}
						></AgGridReact>
					</div>
				</Box>
			</Layout>
			<Snackbar
				open={open}
				autoHideDuration={20000}
				anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
			>
				<Alert onClose={handleClose} severity='error'>
	<span>{toastrmsg}</span>
				</Alert>
			</Snackbar>
		</React.Fragment>
	);
};

const mapStateToProps = (state: any) => {
	console.log('state',state)
	const { company } = state;
	return {
		loading: company.loading,
		items: company.data,
		error: company.error,
		Companies: company.Companies,
	};
};

export default connect(mapStateToProps, {
	selectedCompany,
	setCompanies,
	fetchCompanies,
})(Authenticate(Home));
